provider "google" {
  project = var.project_id
  region  = var.project_region
  zone    = var.project_zone
}

provider "github" {
  token = var.gh_token
  owner = var.repository_owner
}

resource "google_service_account" "sa" {
  project    = var.project_id
  account_id = "test-storage-sa"
}

resource "google_project_iam_member" "project" {
  project = var.project_id
  role    = "roles/storage.admin"
  member = "serviceAccount:${google_service_account.sa.email}"
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
    #     "POOL_ID" : google_iam_workload_identity_pool.main.workload_identity_pool_id,
    #     "PROVIDER_ID" : google_iam_workload_identity_pool_provider.main.workload_identity_pool_provider_id,
    "APPLICATION_ID" : var.application_id,
    "ANDROID_SIGNING_KEY_STORE" : var.android_signing_key_store,
    "ANDROID_SIGNING_KEY_ALIAS" : var.android_signing_key_alias,
    "ANDROID_FRAMEWORK" : var.android_framework,
    "USER_IDENTIFIER" : var.user_identifier,
    #     "WORKLOAD_IDENTITY_PROVIDER" : local.workload_identity_provider,
    "REPOSITORY_OWNER" : var.repository_owner,
    "REPOSITORY_NAME" : var.repository_name,
    "PROJECT_ID" : var.project_id,
    "PROJECT_REGION" : var.project_region,
    "PROJECT_ZONE" : var.project_zone
  })
  repository    = var.repository_name
  variable_name = each.key
  value         = each.value
}

module "oidc" {
  source  = "terraform-google-modules/github-actions-runners/google//modules/gh-oidc"
  version = "~> 4.0"

  project_id          = var.project_id
  pool_id             = "example-pool"
  provider_id         = "example-gh-provider"
  attribute_condition = "assertion.repository_owner=='tbui17'"
  sa_mapping = {
    (google_service_account.sa.account_id) = {
      sa_name   = google_service_account.sa.name
      attribute = "attribute.repository/tbui17/CoursePlanner"
    }
  }
}

locals {
  attribute_repository_const        = "attribute.repository"
  github_actions_const              = "github-actions"
  github_actions_display_name_const = "GitHub Actions"
  #   workload_identity_provider = "projects/${var.project_id}/locations/global/workloadIdentityPools/${google_iam_workload_identity_pool.main.workload_identity_pool_id}/providers/${google_iam_workload_identity_pool_provider.main.workload_identity_pool_id}"
}
