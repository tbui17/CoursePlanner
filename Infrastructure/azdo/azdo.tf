
terraform {
  required_providers {
    azuredevops = {
      source  = "microsoft/azuredevops"
      version = "1.3.0"
    }
    corefunc = {
      source  = "northwood-labs/corefunc"
      version = "1.4.0"
    }

  }
}

resource "azuredevops_project" "this" {
  description        = null
  name               = provider::corefunc::str_pascal(var.repository_name, false)
  version_control    = "Git"
  visibility         = "private"
  work_item_template = "Agile"
}
