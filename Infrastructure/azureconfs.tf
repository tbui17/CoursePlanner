
resource "azurerm_app_configuration_key" "appconf" {
  configuration_store_id = azurerm_app_configuration.appconf.id
  for_each               = { for k, v in local.variables : provider::corefunc::str_pascal(k, false) => v }
  key                    = each.key
  value                  = each.value
  depends_on             = [azurerm_role_assignment.appconf_dataowner]
}

resource "azurerm_key_vault_secret" "this" {
  key_vault_id = azurerm_key_vault.key_vault.id
  for_each     = local.azure_key_vaults
  name         = each.key
  value        = each.value
}

locals {

  _delimiter       = "4ba72925-6744-4158-93a0-c2f5fbaf3a83"
  _azure_delimiter = "--"

  _primitives = {
    for k, v in local.secrets : k => v if !can(tomap(v))
  }

  # 1-level-deep object flattening, represent nested objects with delimited combined keys
  _flattened = merge([
    for k, v in local.secrets : { for k2, v2 in v : "${k}${local._delimiter}${k2}" => v2 }
    if can(tomap(v))
  ]...)

  _all = merge(local._flattened, local._primitives)

  # convert keys to PascalCase and mark values as sensitive
  azure_key_vaults = { for k, v in local._all :
    join("${local._azure_delimiter}",
      [for str in split("${local._delimiter}", k) : provider::corefunc::str_pascal(str, false)]
    )
    => sensitive(v)
  }
}
