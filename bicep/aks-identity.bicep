param location string = resourceGroup().location
param prefix string = resourceGroup().name
param contributorRoleDefinitionId string
param tags object

resource aksManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2018-11-30' = {
  name: '${prefix}-aks-managed-identity'
  location: location
  tags: tags
}

resource roleassignment 'Microsoft.Authorization/roleAssignments@2020-08-01-preview' = {
  name: guid(contributorRoleDefinitionId, resourceGroup().id)
  scope: resourceGroup()

  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', contributorRoleDefinitionId)
    principalId: aksManagedIdentity.properties.principalId
  }
}

output principalId string = aksManagedIdentity.properties.principalId
output id string = aksManagedIdentity.id
