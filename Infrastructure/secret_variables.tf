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

