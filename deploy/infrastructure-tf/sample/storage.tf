resource "azurerm_resource_group" "rg" {
  name     = "rg-devops-sample"
  location = "eastus"
}

resource "azurerm_storage_account" "storage" {
  name                     = "stdevopssample${var.environment}"
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "RAGZRS"
}