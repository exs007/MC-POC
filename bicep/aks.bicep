param location string = resourceGroup().location
param prefix string = resourceGroup().name
param tags object
param msiId string

var vNetName = '${prefix}-aks-vnet'
var subnetName = '${prefix}-aks-subnet'
var subnetRef = '${aksVNet.id}/subnets/${subnetName}'

resource aksNsg 'Microsoft.Network/networkSecurityGroups@2021-02-01' = {
  location: location
  name: '${prefix}-aks-nsg'
  properties: {
    securityRules: [
      {
        name: 'FD-Traffic-Allowed'
        properties: {
          access: 'Allow'
          destinationAddressPrefix: '*'
          destinationAddressPrefixes: []
          destinationPortRanges: [
            '80'
            '443'
          ]
          direction: 'Inbound'
          priority: 100
          protocol: 'Tcp'
          sourceAddressPrefix: 'AzureFrontDoor.Backend'
          sourceAddressPrefixes: []
          sourcePortRange: '*'
          sourcePortRanges: []
        }
      }
    ]
  }
}

resource aksVNet 'Microsoft.Network/virtualNetworks@2021-02-01' = {
  location: 'eastus'
  name: vNetName
  properties: {
    addressSpace: {
      addressPrefixes: [
        '192.168.0.0/16'
      ]
    }
    subnets: [
      {
        name: subnetName
        properties: {
          addressPrefix: '192.168.1.0/24'
          networkSecurityGroup: {
            id: aksNsg.id
          }
        }
      }
    ]
    virtualNetworkPeerings: []
  }
}

resource aks 'Microsoft.ContainerService/managedClusters@2021-03-01' = {
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${msiId}': {}
    }
  }
  location: location
  name: '${prefix}-aks'
  sku: {
    name: 'Basic'
    tier: 'Free'
  }
  tags: tags
  properties: {    
    agentPoolProfiles: [
      {
        count: 1
        kubeletDiskType: 'OS'
        maxPods: 30
        mode: 'System'
        name: 'default'
        type: 'VirtualMachineScaleSets'
        orchestratorVersion: '1.19.11'
        osDiskSizeGB: 32
        osDiskType: 'Managed'
        osSKU: 'Ubuntu'
        osType: 'Linux'
        vmSize: 'Standard_DS2_v2'
        vnetSubnetID: subnetRef
      }      
    ]    
    kubernetesVersion: '1.19.11'
    dnsPrefix: 'aks'
    nodeResourceGroup: '${prefix}-aks-node-rg'
    networkProfile: {
      loadBalancerProfile: {
        managedOutboundIPs: {
          count: 1
        }
      }
      dnsServiceIP: '10.0.0.10'
      dockerBridgeCidr: '172.17.0.1/16'
      loadBalancerSku: 'standard'
      networkPlugin: 'azure'
      outboundType: 'loadBalancer'
      serviceCidr: '10.0.0.0/16'
    }
    servicePrincipalProfile: {
      clientId: 'msi'
    }
  }
}

output kubeletIdentityOjectId string = aks.properties.identityProfile.kubeletidentity.objectId

