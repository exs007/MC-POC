param location string = resourceGroup().location
param prefix string = resourceGroup().name
param tags object

resource aksIngressIp 'Microsoft.Network/publicIPAddresses@2021-02-01' = {
  name: '${prefix}-aks-ingress-ip'
  location: location
  sku: {
    name: 'Standard'
    tier: 'Regional'
  }
  properties: {
    publicIPAllocationMethod: 'Static'
    publicIPAddressVersion: 'IPv4'
  }
  tags: tags
}

// Set an output which can be accessed by the module consumer
output ipAddress string = aksIngressIp.properties.ipAddress
