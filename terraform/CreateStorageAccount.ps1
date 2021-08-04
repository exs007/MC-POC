param(
    [string] $location = 'eastus',
    [Parameter(Mandatory=$true)]
    [string] $groupName = 'main',
    [Parameter(Mandatory=$true)]
    [string] $storageAccount='maintfstorageaccount'
)

az group create --location $location --name $groupName
az storage account create --name $storageAccount --resource-group $groupName --location $location --sku Standard_LRS
$json = az storage account keys list -g $groupName -n $storageAccount | ConvertFrom-Json
$key = $json[0].value
az storage container create --name tfcontainer --account-name $storageAccount --account-key $key
Write-Host Access Key: $key