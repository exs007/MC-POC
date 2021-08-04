param location string = resourceGroup().location
param prefix string = resourceGroup().name
param sqlServerAdmin string
@secure()
param sqlServerPass string
param tags object

resource server 'Microsoft.Sql/servers@2021-02-01-preview' = {
  location: location
  name: '${prefix}-sql'
  properties: {
    administratorLogin: sqlServerAdmin
    administratorLoginPassword: sqlServerPass
    publicNetworkAccess: 'Enabled'
    restrictOutboundNetworkAccess: 'Disabled'
    version: '12.0'
  }
  tags: tags
}

resource serverFirewallRuleAzure 'Microsoft.Sql/servers/firewallRules@2021-02-01-preview' = {
  parent: server
  name: '${server.name}-firewall-rule-azure'
  properties: {
    endIpAddress: '0.0.0.0'
    startIpAddress: '0.0.0.0'
  }
}

output serverName string = server.name
