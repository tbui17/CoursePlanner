provider "google" {
  project = var.project_id
  region  = var.project_region
  zone    = var.project_zone
}

provider "github" {
  token = var.gh_token
  owner = var.repository_owner
}

resource "google_service_account" "github_actions" {
  account_id   = "${local.github_actions_const}-account"
  display_name = "${local.github_actions_display_name_const} Account"
}

resource "google_iam_workload_identity_pool" "main" {
  provider                  = google-beta
  project                   = var.project_id
  workload_identity_pool_id = "${local.github_actions_const}-pool2"
  display_name              = "${local.github_actions_display_name_const} Pool"
  disabled                  = false
}

resource "google_service_account_iam_member" "wif-sa" {
  service_account_id = google_service_account.github_actions.name
  role               = "roles/iam.workloadIdentityUser"
  member             = "principalSet://iam.googleapis.com/${google_iam_workload_identity_pool.main.name}/${local.attribute_repository_const}/${var.repository_owner}/${var.repository_name}"
}

resource "google_project_iam_member" "project" {
  project = var.project_id
  role    = "roles/storage.admin"
  member  = "serviceAccount:${google_service_account.github_actions.email}"
}

resource "google_iam_workload_identity_pool_provider" "main" {
  provider                           = google-beta
  project                            = var.project_id
  workload_identity_pool_id          = google_iam_workload_identity_pool.main.workload_identity_pool_id
  workload_identity_pool_provider_id = "${local.github_actions_const}-provider"
  display_name                       = "${local.github_actions_display_name_const} Provider"
  attribute_condition                = "assertion.repository_owner=='${var.repository_owner}'"
  attribute_mapping = {
    "google.subject"             = "assertion.sub"
    "attribute.actor"            = "assertion.actor"
    (local.attribute_repository_const) = "assertion.repository"
    "attribute.repository_owner" = "assertion.repository_owner"
  }
  oidc {
    issuer_uri = "https://token.actions.githubusercontent.com"
  }
}

data "github_actions_public_key" "github_public_key" {
  repository = "${var.repository_name}"
}

resource "github_actions_secret" "action_secret" {
  for_each = tomap({
    "KEY" : var.key,
    "GOOGLE_SERVICE_ACCOUNT" : jsonencode(var.google_service_account),
    "KEYSTORE_CONTENTS" : var.keystore_contents,
    "GOOGLE_SERVICE_ACCOUNT_BASE64" : var.google_service_account_base64,
    "KEY_URI" : var.key_uri
  })
  repository      = var.repository_name
  secret_name     = each.key
  plaintext_value = each.value
}

resource "github_actions_variable" "action_var" {
  for_each = tomap({
    "PROJECT_NUMBER" : var.project_number,
    "POOL_ID" : google_iam_workload_identity_pool.main.workload_identity_pool_id,
    "PROVIDER_ID" : google_iam_workload_identity_pool_provider.main.workload_identity_pool_provider_id,
    "APPLICATION_ID" : var.application_id,
    "ANDROID_SIGNING_KEY_STORE" : var.android_signing_key_store,
    "ANDROID_SIGNING_KEY_ALIAS" : var.android_signing_key_alias,
    "ANDROID_FRAMEWORK" : var.android_framework,
    "USER_IDENTIFIER" : var.user_identifier,
    "WORKLOAD_IDENTITY_PROVIDER" : local.workload_identity_provider,
  })
  repository    = var.repository_name
  variable_name = each.key
  value         = each.value
}
#
# module "gh_oidc" {
#   source      = "terraform-google-modules/github-actions-runners/google//modules/gh-oidc"
#   project_id  = var.project_id
#   pool_id     = "${local.github_actions_const}-pool2"
#   provider_id = "${local.github_actions_const}-provider"
#   sa_mapping = {
#     (google_service_account.github_actions.account_id) = {
#       sa_name   = "projects/courseplanner-437101/serviceAccounts/github-actions-account@courseplanner-437101.iam.gserviceaccount.com"
#       attribute = "attribute.repository/tbui17/CoursePlanner"
#     }
#   }
# }

locals {
  attribute_repository_const        = "attribute.repository"
  github_actions_const              = "github-actions"
  github_actions_display_name_const = "GitHub Actions"
  workload_identity_provider = "projects/${var.project_id}/locations/global/workloadIdentityPools/${google_iam_workload_identity_pool.main.workload_identity_pool_id}/providers/${google_iam_workload_identity_pool_provider.main.workload_identity_pool_id}"
}
