resource "github_actions_secret" "action_secret" {
  for_each = tomap({
    "KEY" : var.key,
    "GOOGLE_SERVICE_ACCOUNT" : jsonencode(var.google_service_account),
    "KEYSTORE_CONTENTS" : var.keystore_contents,
    "GOOGLE_SERVICE_ACCOUNT_BASE64" : var.google_service_account_base64,
    "KEY_URI" : var.key_uri
    "AZURE_CREDENTIALS" : local.AZURE_CREDENTIALS,
  })
  repository      = var.repository_name
  secret_name     = each.key
  plaintext_value = each.value
}

resource "github_actions_variable" "action_var" {
  for_each = local.github_action_variables
  repository    = var.repository_name
  variable_name = each.key
  value         = each.value
}

locals {
  AZURE_CREDENTIALS = jsonencode({
    subscriptionId = var.subscription_id,
    tenantId       = var.tenant_id,
    clientId       = var.service_principal_id,
    clientSecret   = var.service_principal_secret
  })
  _github_action_variables1 = {for k, v in local.variables : upper(k) => v}
  _github_action_variables2 = {

    "POOL_ID" : google_iam_workload_identity_pool.main.workload_identity_pool_id,
    "PROVIDER_ID" : google_iam_workload_identity_pool_provider.main.workload_identity_pool_provider_id,
    "WORKLOAD_IDENTITY_PROVIDER" : google_iam_workload_identity_pool_provider.main.name,
    "RELEASE_FILES" : local.release_files,
    "OUTPUT_FOLDER" : local.output_folder
  }
  github_action_variables = merge(local._github_action_variables1, local._github_action_variables2)
}