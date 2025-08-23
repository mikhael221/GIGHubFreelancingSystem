# Azure ML Studio Setup Script for Smart Hiring
# Run this in Azure Cloud Shell or Azure CLI

# Variables - Update these with your values
$resourceGroupName = "rg-freelancing-ml"
$location = "eastus"
$workspaceName = "ml-smart-hiring"
$storageAccountName = "smarthiringstorage$(Get-Random -Minimum 1000 -Maximum 9999)"
$keyVaultName = "kv-smart-hiring-$(Get-Random -Minimum 1000 -Maximum 9999)"
$containerRegistryName = "acrsmrthiring$(Get-Random -Minimum 1000 -Maximum 9999)"

# Create Resource Group
Write-Host "Creating Resource Group..." -ForegroundColor Green
az group create --name $resourceGroupName --location $location

# Create Storage Account
Write-Host "Creating Storage Account..." -ForegroundColor Green
az storage account create `
    --name $storageAccountName `
    --resource-group $resourceGroupName `
    --location $location `
    --sku Standard_LRS `
    --kind StorageV2

# Create Key Vault
Write-Host "Creating Key Vault..." -ForegroundColor Green
az keyvault create `
    --name $keyVaultName `
    --resource-group $resourceGroupName `
    --location $location

# Create Container Registry
Write-Host "Creating Container Registry..." -ForegroundColor Green
az acr create `
    --name $containerRegistryName `
    --resource-group $resourceGroupName `
    --sku Basic `
    --admin-enabled true

# Create ML Workspace
Write-Host "Creating ML Workspace..." -ForegroundColor Green
az ml workspace create `
    --workspace-name $workspaceName `
    --resource-group $resourceGroupName `
    --location $location `
    --storage-account $storageAccountName `
    --key-vault $keyVaultName `
    --container-registry $containerRegistryName

# Create compute cluster for training
Write-Host "Creating compute cluster..." -ForegroundColor Green
az ml compute create `
    --workspace-name $workspaceName `
    --resource-group $resourceGroupName `
    --name "smart-hiring-cluster" `
    --type amlcompute `
    --size Standard_DS3_v2 `
    --min-instances 0 `
    --max-instances 4

Write-Host "Azure ML Setup Complete!" -ForegroundColor Green
Write-Host "Workspace Name: $workspaceName"
Write-Host "Resource Group: $resourceGroupName"
Write-Host "Storage Account: $storageAccountName"

