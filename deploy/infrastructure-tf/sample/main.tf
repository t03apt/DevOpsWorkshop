terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "= 3.61"
    }
  }

  backend "azurerm" {
    resource_group_name  = "rg-devops-terraform-state"
    storage_account_name = "stdevopstfstatedev"
    container_name       = "tfstate"
    key                  = "dev"

    access_key = "OdAIOwfMSa8wWeRki/9Cb5wwhJAb/3kIDoq/uYD4u8lK5aGWmuppuuw6TS7kA2aKLljb+gnevnO/+AStRbXTbQ=="
  }

  required_version = ">= 1.0.4"
}

provider "azurerm" {
  features {}
}
