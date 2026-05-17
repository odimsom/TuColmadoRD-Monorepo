terraform {
  required_version = ">= 1.6"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.110"
    }
  }
}

variable "location"      { default = "East US" }
variable "vm_size"       { default = "Standard_B1ms" }
variable "admin_user"    { default = "azureuser" }
variable "ssh_public_key" { description = "SSH public key content" }

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "tucolmadord" {
  name     = "rg-tucolmadord"
  location = var.location
}

resource "azurerm_virtual_network" "tucolmadord" {
  name                = "vnet-tucolmadord"
  address_space       = ["10.0.0.0/16"]
  location            = azurerm_resource_group.tucolmadord.location
  resource_group_name = azurerm_resource_group.tucolmadord.name
}

resource "azurerm_subnet" "workers" {
  name                 = "snet-workers"
  resource_group_name  = azurerm_resource_group.tucolmadord.name
  virtual_network_name = azurerm_virtual_network.tucolmadord.name
  address_prefixes     = ["10.0.1.0/24"]
}

resource "azurerm_public_ip" "worker" {
  name                = "pip-tucolmadord-worker"
  location            = azurerm_resource_group.tucolmadord.location
  resource_group_name = azurerm_resource_group.tucolmadord.name
  allocation_method   = "Static"
  sku                 = "Standard"
}

resource "azurerm_network_interface" "worker" {
  name                = "nic-worker"
  location            = azurerm_resource_group.tucolmadord.location
  resource_group_name = azurerm_resource_group.tucolmadord.name

  ip_configuration {
    name                          = "internal"
    subnet_id                     = azurerm_subnet.workers.id
    private_ip_address_allocation = "Dynamic"
    public_ip_address_id          = azurerm_public_ip.worker.id
  }
}

resource "azurerm_linux_virtual_machine" "worker" {
  name                = "vm-tucolmadord-worker"
  resource_group_name = azurerm_resource_group.tucolmadord.name
  location            = azurerm_resource_group.tucolmadord.location
  size                = var.vm_size
  admin_username      = var.admin_user

  network_interface_ids = [azurerm_network_interface.worker.id]

  admin_ssh_key {
    username   = var.admin_user
    public_key = var.ssh_public_key
  }

  os_disk {
    caching              = "ReadWrite"
    storage_account_type = "Standard_LRS"
  }

  source_image_reference {
    publisher = "Canonical"
    offer     = "0001-com-ubuntu-server-jammy"
    sku       = "22_04-lts-gen2"
    version   = "latest"
  }

  custom_data = base64encode(<<-EOF
    #!/bin/bash
    apt-get update -qq
    apt-get install -y -qq docker.io docker-compose-plugin
    systemctl enable --now docker
  EOF
  )

  tags = {
    Project = "TuColmadoRD"
    Role    = "worker"
  }
}

output "worker_public_ip" {
  value = azurerm_public_ip.worker.ip_address
}
