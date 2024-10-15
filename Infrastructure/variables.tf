variable "repository_owner" {
  description = "The owner of the GitHub repository"
}

variable "repository_name" {
  description = "The name of the GitHub repository"
}

variable "project_id" {
  description = "The ID for the Google Cloud project"
}

variable "project_region" {
  description = "The region for the Google Cloud project"
}

variable "project_zone" {
  description = "The zone for the Google Cloud project"
}

variable "project_number" {
  description = "The project number for the Google Cloud project"
}

variable "application_id" {
  description = "The application ID for the Android app"
}

variable "android_signing_key_store" {
  description = "The path to the Android signing key store"
}

variable "android_signing_key_alias" {
  description = "The alias for the Android signing key"
}

variable "android_framework" {
  description = "The framework for the Android app"
}

variable "user_identifier" {
  description = "The identifier for the runner job"
}

variable "blob_container_name" {
  description = "The name of the blob container"
}

variable "devops_org_url" {
  description = "The Azure DevOps organization URL"
  type        = string
  sensitive   = true
}

variable solution_name {
  description = "The name of the solution"
  type        = string
}

variable project_name {
  description = "The name of the project to be built"
  type        = string
}

variable app_version {
  description = "The version of the app"
  type        = string
  validation {
    condition = can(regex(
      "^(?P<major>0|[1-9]\\d*)\\.(?P<minor>0|[1-9]\\d*)\\.(?P<patch>0|[1-9]\\d*)(?:-(?P<prerelease>(?:0|[1-9]\\d*|\\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\\.(?:0|[1-9]\\d*|\\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\\+(?P<buildmetadata>[0-9a-zA-Z-]+(?:\\.[0-9a-zA-Z-]+)*))?$",
      var.app_version))
    error_message = "App version must comply with semantic versioning format i.e. 1.0.0"
  }
}

variable publish_configuration {
  description = "The configuration mode for dotnet publish when building the project"
  type        = string
}

variable workspace_name {
  description = "The name of the workspace"
  type        = string
}

locals {
  variables = {
    repository_owner          = var.repository_owner
    repository_name           = var.repository_name
    project_id                = var.project_id
    project_region            = var.project_region
    project_zone              = var.project_zone
    project_number            = var.project_number
    application_id            = var.application_id
    android_signing_key_store = var.android_signing_key_store
    android_signing_key_alias = var.android_signing_key_alias
    android_framework         = var.android_framework
    user_identifier           = var.user_identifier
    blob_container_name   = var.blob_container_name
    devops_org_url        = var.devops_org_url
    solution_name         = var.solution_name
    project_name          = var.project_name
    app_version           = var.app_version
    publish_configuration = var.publish_configuration
  }
}