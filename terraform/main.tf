# Configure the Microsoft Azure Provider.
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 2.26"
    }
  }
  required_version = ">= 0.14.9"
  backend "azurerm" {
    container_name = "tfcontainer"
    key            = "terraform.tfstate"
  }
}

provider "azurerm" {
  features {}
}

# Create a resource group
resource "azurerm_resource_group" "rg" {
  name     = var.prefix
  location = var.location
  tags     = var.tags
}

# acr
resource "azurerm_container_registry" "acr" {
  name                = var.acr_name
  resource_group_name = azurerm_resource_group.main-rg.name
  location            = azurerm_resource_group.rg.location
  sku                 = "Standard"
  admin_enabled       = false
  tags                = var.tags
}

# public aks ip
resource "azurerm_public_ip" "aks-ingress-ip" {
  name                = "${var.prefix}-aks-ingress-ip"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  allocation_method   = "Static"
  sku                 = "Standard"
  tags                = var.tags
}

# seq disk for aks
resource "azurerm_managed_disk" "aks-seq-disc" {
  name                 = "${var.prefix}-aks-seq-disk"
  resource_group_name  = azurerm_resource_group.rg.name
  location             = azurerm_resource_group.rg.location
  storage_account_type = "Premium_LRS"
  create_option        = "Empty"
  disk_size_gb         = "8"
  tags                 = var.tags
}

# mssql
resource "azurerm_mssql_server" "sql" {
  name                         = "${var.prefix}-sql"
  resource_group_name          = azurerm_resource_group.rg.name
  location                     = azurerm_resource_group.rg.location
  version                      = "12.0"
  administrator_login          = var.sql_server_admin
  administrator_login_password = var.sql_server_pass
  tags                         = var.tags
}

# access from azure resources
resource "azurerm_mssql_firewall_rule" "sql-firewall-rule-azure" {
  name             = "${var.prefix}-sql-firewall-rule-azure"
  server_id        = azurerm_mssql_server.sql.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

# customers db
resource "azurerm_mssql_database" "sql-db-customers" {
  name      = "${var.prefix}-sql-db-customers"
  server_id = azurerm_mssql_server.sql.id
  collation = "SQL_Latin1_General_CP1_CI_AS"
  sku_name  = "S0"
  tags      = var.tags
}

# front door config
resource "azurerm_frontdoor" "fd" {
  name                                         = "${var.prefix}-fd"
  resource_group_name                          = azurerm_resource_group.rg.name
  enforce_backend_pools_certificate_name_check = false

  routing_rule {
    name               = "rule-aks"
    accepted_protocols = ["Https"]
    patterns_to_match  = ["/*"]
    frontend_endpoints = ["fe-aks-api", "fe-aks-logs", "fe-aks-logs-ing", "fe-aks-test"]
    forwarding_configuration {
      forwarding_protocol = "HttpOnly"
      backend_pool_name   = "bp-aks"
    }
  }

  routing_rule {
    name               = "rule-http-https"
    accepted_protocols = ["Http"]
    patterns_to_match  = ["/*"]
    frontend_endpoints = ["fe-aks-api", "fe-aks-logs", "fe-aks-logs-ing", "fe-aks-test"]
    redirect_configuration {
      redirect_type     = "Moved"
      redirect_protocol = "HttpsOnly"
    }
    enabled = false
  }

  backend_pool_load_balancing {
    name = "bp-aks-lb"
  }

  backend_pool_health_probe {
    name         = "bp-aks-hp"
    probe_method = "HEAD"
  }

  backend_pool {
    name = "bp-aks"
    backend {
      host_header = ""
      address     = azurerm_public_ip.aks-ingress-ip.ip_address
      http_port   = 80
      https_port  = 443
    }

    load_balancing_name = "bp-aks-lb"
    health_probe_name   = "bp-aks-hp"
  }

  frontend_endpoint {
    name      = "fe-aks-api"
    host_name = var.host_api
  }

  frontend_endpoint {
    name      = "fe-aks-logs"
    host_name = var.host_logs
  }

  frontend_endpoint {
    name      = "fe-aks-logs-ing"
    host_name = "ingestion.${var.host_logs}"
  }

  frontend_endpoint {
    name      = "fe-aks-test"
    host_name = "${var.prefix}-fd.azurefd.net"
  }
}

# ssl certs setup
resource "azurerm_frontdoor_custom_https_configuration" "fe-aks-api-https-configuration" {
  frontend_endpoint_id              = azurerm_frontdoor.fd.frontend_endpoints["fe-aks-api"]
  custom_https_provisioning_enabled = true

  custom_https_configuration {
    certificate_source = "FrontDoor"
  }
}

resource "azurerm_frontdoor_custom_https_configuration" "fe-aks-logs-https-configuration" {
  frontend_endpoint_id              = azurerm_frontdoor.fd.frontend_endpoints["fe-aks-logs"]
  custom_https_provisioning_enabled = true

  custom_https_configuration {
    certificate_source = "FrontDoor"
  }
}

resource "azurerm_frontdoor_custom_https_configuration" "fe-aks-logs-ing-https-configuration" {
  frontend_endpoint_id              = azurerm_frontdoor.fd.frontend_endpoints["fe-aks-logs-ing"]
  custom_https_provisioning_enabled = true

  custom_https_configuration {
    certificate_source = "FrontDoor"
  }
}

# aks managed identity
resource "azurerm_user_assigned_identity" "aks-managed-identity" {
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  name                = "${var.prefix}-aks-managed-identity"
  tags                = var.tags
}

# access to the resource group
resource "azurerm_role_assignment" "tc-acr" {
  scope                = azurerm_resource_group.rg.id
  role_definition_name = "Contributor"
  principal_id         = azurerm_user_assigned_identity.aks-managed-identity.principal_id
}

resource "azurerm_role_assignment" "aks-role-network-contributor" {
  scope                = azurerm_resource_group.rg.id
  role_definition_name = "Network Contributor"
  principal_id         = azurerm_user_assigned_identity.aks-managed-identity.principal_id
}

# aks network
resource "azurerm_virtual_network" "aks-vnet" {
  name                = "${var.prefix}-aks-vnet"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  address_space       = ["192.168.0.0/16"]
}

resource "azurerm_subnet" "aks-subnet" {
  name                 = "${var.prefix}-aks-subnet"
  resource_group_name  = azurerm_resource_group.rg.name
  address_prefixes     = ["192.168.1.0/24"]
  virtual_network_name = azurerm_virtual_network.aks-vnet.name
}

resource "azurerm_network_security_group" "aks-nsg" {
  name                = "${var.prefix}-aks-nsg"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location

  security_rule {
    name                       = "FD-Traffic-Allowed"
    priority                   = 100
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_ranges    = ["80", "443"]
    source_address_prefix      = "AzureFrontDoor.Backend"
    destination_address_prefix = "*"
  }
}

resource "azurerm_subnet_network_security_group_association" "aks-subnet-to-nsg" {
  subnet_id                 = azurerm_subnet.aks-subnet.id
  network_security_group_id = azurerm_network_security_group.aks-nsg.id
}

# aks
resource "azurerm_kubernetes_cluster" "aks" {
  name                = "${var.prefix}-aks"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  dns_prefix          = "aks"

  default_node_pool {
    name           = "default"
    node_count     = 1
    vm_size        = "Standard_DS2_v2"
    vnet_subnet_id = azurerm_subnet.aks-subnet.id
  }

  network_profile {
    network_plugin    = "azure"
    load_balancer_sku = "Standard"
  }

  identity {
    type                      = "UserAssigned"
    user_assigned_identity_id = azurerm_user_assigned_identity.aks-managed-identity.id
  }
  tags = var.tags
}

# grant acr pull permissions to the pool managed identity
resource "azurerm_role_assignment" "aks-to-acr" {
  scope                = azurerm_container_registry.acr.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_kubernetes_cluster.aks.kubelet_identity[0].object_id
}