param location string = resourceGroup().location
param name string
param tags object
param tier string
param sku string
param diskSizeGB int

resource aksSeqDisk 'Microsoft.Compute/disks@2020-12-01' = {
  name: name
  location: location
  properties: {
    creationData: {
      createOption: 'Empty'
    }
    networkAccessPolicy: 'AllowAll'
    tier: tier
    diskSizeGB: diskSizeGB
  }
  sku: {
    name: sku
  }
  tags: tags
}
