resource "azurerm_resource_group" "this" {
  location   = "westcentralus"
  managed_by = null
  name = var.repository_owner
  tags = {}
}

resource "azurerm_key_vault" "key_vault" {
  access_policy = []
  enable_rbac_authorization       = true
  enabled_for_deployment          = true
  enabled_for_disk_encryption     = true
  enabled_for_template_deployment = true
  location                        = "westcentralus"
  name                = azurerm_resource_group.this.name
  public_network_access_enabled   = true
  purge_protection_enabled        = false
  resource_group_name = azurerm_resource_group.this.name
  sku_name                        = "standard"
  soft_delete_retention_days      = 90
  tags = {
    secrets = ""
  }
  tenant_id = var.tenant_id
  network_acls {
    bypass         = "AzureServices"
    default_action = "Allow"
    ip_rules = []
    virtual_network_subnet_ids = []
  }
}

resource "azurerm_storage_account" "storage" {
  name                = "${azurerm_resource_group.this.name}storage"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  account_tier             = "Standard"
  account_replication_type = "LRS"


  tags = {
    storage = ""
  }
}

resource "azurerm_storage_container" "storage" {
  name                  = "storage"
  storage_account_name  = azurerm_storage_account.storage.name
  container_access_type = "private"
}

data "azurerm_client_config" "current" {}


resource "azurerm_monitor_diagnostic_setting" "logging" {
  name               = "logging"
  target_resource_id = azurerm_key_vault.key_vault.id
  storage_account_id = azurerm_storage_account.storage.id

  enabled_log {
    category = "AuditEvent"
  }

  metric {
    category = "AllMetrics"
  }
}

resource "azurerm_log_analytics_workspace" "logging" {
  name                = "logging"
  location            = azurerm_resource_group.this.location
  resource_group_name = azurerm_resource_group.this.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
  daily_quota_gb      = 0.5
}

resource "azurerm_app_configuration" "appconf" {
  name                = "${azurerm_resource_group.this.name}appconf"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
}

resource "azurerm_role_assignment" "appconf_dataowner" {
  scope                = azurerm_app_configuration.appconf.id
  role_definition_name = "App Configuration Data Owner"
  principal_id         = data.azurerm_client_config.current.object_id
}


data "azuread_client_config" "current" {}


resource "azuread_application" "terraform" {
  display_name = "terraform"
  owners = [data.azuread_client_config.current.object_id]
}

resource "azuread_service_principal" "terraform" {
  client_id = azuread_application.terraform.client_id
  owners = [data.azuread_client_config.current.object_id]
}