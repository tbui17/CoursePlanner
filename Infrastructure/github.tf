data "github_actions_public_key" "github_public_key" {
  repository = var.repository_name
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
    "WORKLOAD_IDENTITY_PROVIDER" : google_iam_workload_identity_pool_provider.main.name,
    "REPOSITORY_OWNER" : var.repository_owner,
    "REPOSITORY_NAME" : var.repository_name,
    "PROJECT_ID" : var.project_id,
    "PROJECT_REGION" : var.project_region,
    "PROJECT_ZONE" : var.project_zone,
    "RELEASE_FILES" : local.release_files,
  })
  repository    = var.repository_name
  variable_name = each.key
  value         = each.value
}