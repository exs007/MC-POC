targetScope = 'subscription'

// main
param environment string = 'DEV'
param groupName string
param groupLocation string

param tags object = {
  Environment: environment
}

// existing, group?
param acrName string = '{ACR}'
param dnsZoneName string = '{DNS_ZONE}'
param kvName string = '{KEY_VAULT}'
param mainResourceGroupName string = '{MAIN_GROUP_NAME}'

// sql
param sqlServerAdmin string = '{SQL_USER}'

// domain names
param subDomainApi string = 'api2'
param subDomainLogs string = 'logs2'

resource group 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: groupName
  location: groupLocation
}

// role ids
module roles './roles.bicep' = {
  name: 'roles'  
  scope: resourceGroup(groupName)
}

// key-vault to get senstive settings
resource kv 'Microsoft.KeyVault/vaults@2021-04-01-preview' existing = {
  name: kvName
  scope: resourceGroup(mainResourceGroupName)
}

module sql './sql.bicep' = {
  name: 'sql'
  scope: resourceGroup(groupName)
  params: {
    sqlServerAdmin: sqlServerAdmin
    sqlServerPass: kv.getSecret('SQLPASS')
    tags: tags
  }
}

module sqlDBcustomers './sql-db-customers.bicep' = {
  name: 'sql-db-customers'
  scope: resourceGroup(groupName)
  params: {
    serverName: sql.outputs.serverName
    tags: tags
  }
}

module aksIngressIP './aks-ingress-ip.bicep' = {
  name: 'aks-ingress-ip'
  scope: resourceGroup(groupName)
  params: {
    tags: tags
  }
}

module dnsSetupFrontDoorVerify './dnsSetupFrontDoorVerify.bicep' = {
  name: 'dnsSetupFrontDoorVerify'  
  scope: resourceGroup(mainResourceGroupName)
  params: {
    subDomainApi: subDomainApi
    dnsZoneName: dnsZoneName
    subDomainLogs: subDomainLogs
    prefix: groupName
  }
}

module frontDoor './frontDoor.bicep' = {
  name: 'frontDoor'  
  scope: resourceGroup(groupName)
  params: {
    tags: tags
    aksIngressIp: aksIngressIP.outputs.ipAddress    
    subDomainApi: subDomainApi
    dnsZoneName: dnsZoneName
    subDomainLogs: subDomainLogs
  }
}

module dnsSetupFrontDoorLinks './dnsSetupFrontDoorLinks.bicep' = {
  name: 'dnsSetupFrontDoorLinks'  
  scope: resourceGroup(mainResourceGroupName)
  params: {
    subDomainApi: subDomainApi
    dnsZoneName: dnsZoneName
    subDomainLogs: subDomainLogs
    frontDoorId: frontDoor.outputs.id
  }
}

module aksIdentity './aks-identity.bicep' = {
  name: 'aksIdentity'  
  scope: resourceGroup(groupName)
  params: {
    tags: tags
    contributorRoleDefinitionId: roles.outputs.roles.Contributor
  }
}

module aksSeqDisk './disk.bicep' = {
  name: 'aksSeqDisk'  
  scope: resourceGroup(groupName)
  params: {
    name: '${groupName}-aks-seq-disk'
    tier: 'P2'
    sku: 'Premium_LRS'
    diskSizeGB: 8
    tags: tags
  }
}

module aks './aks.bicep' = {
  name: 'aks'  
  scope: resourceGroup(groupName)
  params: {
    msiId: aksIdentity.outputs.id
    tags: tags
  }
}

module aksAcrPull './aks-acr-pill.bicep' = {
  name: 'aksAcrPull'  
  scope: resourceGroup(mainResourceGroupName)
  params: {
    kubeletIdentityOjectId: aks.outputs.kubeletIdentityOjectId
    acrPullRoleDefinitionId: roles.outputs.roles.AcrPull
    acrName: acrName
  }
}
