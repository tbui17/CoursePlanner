variable "key" {
  description = "Key for the application"
  type        = string
  sensitive   = true
}

variable "google_service_account" {
  description = "Google service account details"
  type = object({
    type                        = string
    project_id                  = string
    private_key_id              = string
    private_key                 = string
    client_email                = string
    client_id                   = string
    auth_uri                    = string
    token_uri                   = string
    auth_provider_x509_cert_url = string
    client_x509_cert_url        = string
    universe_domain             = string
  })
}

variable subscription_id {
  description = "The Azure subscription ID"
  type        = string
  sensitive   = true
}

variable "keystore_contents" {
  description = "Contents of the keystore"
  type        = string
  sensitive   = true
}

variable "google_service_account_base64" {
  description = "Google service account details in base64 format"
  type        = string
  sensitive   = true
}

variable "key_uri" {
  description = "URI for the key"
  type        = string
  sensitive   = true
}

variable "gh_token" {
  description = "The GitHub access token"
  type      = string
  nullable  = false
  sensitive = true
}

variable "tenant_id" {
  description = "The Azure tenant ID"
  type        = string
  sensitive   = true
}

variable "service_principal_id" {
  description = "The Azure service principal ID"
  type        = string
  sensitive   = true
}

variable "service_principal_secret" {
  description = "The Azure service principal secret"
  type        = string
  sensitive   = true
}

variable "connection_string" {
  description = "The connection string for the Azure app configuration"
  type        = string
  sensitive   = true
}

variable "blob_connection_string" {
  description = "The connection string for the Azure blob storage"
  type        = string
  sensitive   = true
}

locals {
  secrets = {
    key                           = var.key
    google_service_account        = var.google_service_account
    subscription_id               = var.subscription_id
    keystore_contents             = var.keystore_contents
    google_service_account_base64 = var.google_service_account_base64
    key_uri                       = var.key_uri
    gh_token                      = var.gh_token
    tenant_id                     = var.tenant_id
    service_principal_id          = var.service_principal_id
    service_principal_secret      = var.service_principal_secret
    connection_string      = var.connection_string
    blob_connection_string = var.blob_connection_string
  }
}