param kubeletIdentityOjectId string
param acrPullRoleDefinitionId string
param acrName string

resource acr 'Microsoft.ContainerRegistry/registries@2020-11-01-preview' existing = {
  name: acrName
}

resource roleassignment 'Microsoft.Authorization/roleAssignments@2020-08-01-preview' = {
  name: guid(acrPullRoleDefinitionId, resourceGroup().id)
  scope: acr

  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', acrPullRoleDefinitionId)
    principalId: kubeletIdentityOjectId
  }
}
