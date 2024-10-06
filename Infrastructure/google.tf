resource "google_service_account" "github_actions" {
  account_id   = "${local.github_actions_const}-account"
  display_name = "${local.github_actions_display_name_const} Account"
}

resource "google_iam_workload_identity_pool" "main" {
  provider                  = google-beta
  project                   = var.project_id
  workload_identity_pool_id = "${local.github_actions_const}-pool5"
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
  workload_identity_pool_provider_id = "${local.github_actions_const}-provider5"
  display_name                       = "${local.github_actions_display_name_const} Provider"
  attribute_condition                = "assertion.repository_owner=='${var.repository_owner}'"
  attribute_mapping = {
    "google.subject"                   = "assertion.sub"
    "attribute.actor"                  = "assertion.actor"
    (local.attribute_repository_const) = "assertion.repository"
    "attribute.repository_owner"       = "assertion.repository_owner"
  }
  oidc {
    issuer_uri = "https://token.actions.githubusercontent.com"
  }
}
