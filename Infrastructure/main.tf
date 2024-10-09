terraform {
  required_providers {
    google = {
      source  = "hashicorp/google"
      version = ">= 6.5.0"
    }
    github = {
      source  = "integrations/github"
      version = "6.3.0"
    }
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 4.4.0"
    }
    corefunc = {
      source  = "northwood-labs/corefunc"
      version = "1.4.0"
    }
  }
}

provider "google" {
  project = var.project_id
  region  = var.project_region
  zone    = var.project_zone
}

provider "github" {
  token = var.gh_token
  owner = var.repository_owner
}

provider "azurerm" {
  features {}
  resource_provider_registrations = "none"
  subscription_id                 = var.subscription_id
}

locals {
  attribute_repository_const        = "attribute.repository"
  github_actions_const              = "github-actions"
  github_actions_display_name_const = "GitHub Actions"
  workload_identity_provider = "projects/${var.project_id}/locations/global/workloadIdentityPools/${google_iam_workload_identity_pool.main.workload_identity_pool_id}/providers/${google_iam_workload_identity_pool_provider.main.workload_identity_pool_id}"
  release_files              = "${local.output_folder}/${local.release_file_name}"
  release_file_name          = "${var.application_id}-Signed.aab"
  output_folder              = "output"
  azure_variables = merge(local.variables, { BundlePath = local.release_file_name })
}
