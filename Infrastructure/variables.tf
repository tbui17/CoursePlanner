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
  }
}