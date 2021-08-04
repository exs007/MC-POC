param location string = resourceGroup().location
param prefix string = resourceGroup().name
param serverName string
param tags object

resource sqlDBCustomers 'Microsoft.Sql/servers/databases@2021-02-01-preview' = {
  name: '${serverName}/${prefix}-sql-db-customers'
  location: location
  sku: {
    name: 'S0'
  }
  tags: tags
}
