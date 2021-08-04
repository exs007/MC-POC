param prefix string = resourceGroup().name
param tags object
param aksIngressIp string
param subDomainApi string
param subDomainLogs string
param dnsZoneName string

var frontDoorName = '${prefix}-fd'
var frontDoorBaseFrontedName = 'fe-aks-base'
var frontDoorApiFrontedName = 'fe-aks-api'
var frontDoorLogsFrontedName = 'fe-aks-logs'
var frontDoorLogsIngestionFrontedName = 'fe-aks-logs-ing'
var frontDoorBaseFrontedHost = '${prefix}-fd.azurefd.net'
var frontDoorApiFrontedHost = '${subDomainApi}.${dnsZoneName}'
var frontDoorLogsFrontedHost = '${subDomainLogs}.${dnsZoneName}'
var frontDoorLogsIngestionFrontedHost = 'ingestion.${subDomainLogs}.${dnsZoneName}'
var loadBalancingSettingsName = 'lb'
var healthProbeSettingsName = 'hp'
var ruleAks = 'rule-aks'
var ruleHttpsTohttp = 'rule-http-https'
var backendPoolName = 'bp-aks'


resource frontDoor 'Microsoft.Network/frontDoors@2020-05-01' = {
  name: '${prefix}-fd'
  location: 'global'
  tags: tags
  properties: {
    enabledState: 'Enabled'

    frontendEndpoints: [
      {
        name: frontDoorBaseFrontedName
        properties: {
          hostName: frontDoorBaseFrontedHost
          sessionAffinityEnabledState: 'Disabled'
        }
      }
      {
        name: frontDoorApiFrontedName
        properties: {
          hostName: frontDoorApiFrontedHost
          sessionAffinityEnabledState: 'Disabled'
        }
        // not supported yet
        // customHttpsConfiguration: {
        //   certificateSource: 'FrontDoor'        
        //   minimumTlsVersion: '1.2'
        //   protocolType: 'ServerNameIndication'
        //   frontDoorCertificateSourceParameters: {
        //     certificateType: 'Dedicated'
        //   }
        // }
      }
      {
        name: frontDoorLogsFrontedName
        properties: {
          hostName: frontDoorLogsFrontedHost
          sessionAffinityEnabledState: 'Disabled'
        }
        // not supported yet
        // customHttpsConfiguration: {
        //   certificateSource: 'FrontDoor'        
        //   minimumTlsVersion: '1.2'
        //   protocolType: 'ServerNameIndication'
        //   frontDoorCertificateSourceParameters: {
        //     certificateType: 'Dedicated'
        //   }
        // }
      }
      {
        name: frontDoorLogsIngestionFrontedName
        properties: {
          hostName: frontDoorLogsIngestionFrontedHost
          sessionAffinityEnabledState: 'Disabled'
        }
        // not supported yet
        // customHttpsConfiguration: {
        //   certificateSource: 'FrontDoor'        
        //   minimumTlsVersion: '1.2'
        //   protocolType: 'ServerNameIndication'
        //   frontDoorCertificateSourceParameters: {
        //     certificateType: 'Dedicated'
        //   }
        // }
      }
    ]

    loadBalancingSettings: [
      {
        name: loadBalancingSettingsName
        properties: {
          sampleSize: 4
          successfulSamplesRequired: 2
        }
      }
    ]

    healthProbeSettings: [
      {
        name: healthProbeSettingsName
        properties: {
          path: '/'
          protocol: 'Http'
          healthProbeMethod: 'HEAD'
          intervalInSeconds: 120
        }
      }
    ]

    backendPools: [
      {
        name: 'bp-aks'
        properties: {
          backends: [
            {
              address: aksIngressIp
              backendHostHeader: ''
              httpPort: 80
              httpsPort: 443
              weight: 50
              priority: 1
              enabledState: 'Enabled'
            }
          ]
          loadBalancingSettings: {
            id: resourceId('Microsoft.Network/frontDoors/loadBalancingSettings', frontDoorName, loadBalancingSettingsName)
          }
          healthProbeSettings: {
            id: resourceId('Microsoft.Network/frontDoors/healthProbeSettings', frontDoorName, healthProbeSettingsName)
          }
        }
      }
    ]

    routingRules: [
      {
        name: ruleAks
        properties: {
          frontendEndpoints: [
            {
              id: resourceId('Microsoft.Network/frontDoors/frontEndEndpoints', frontDoorName, frontDoorBaseFrontedName)
            }            
            {
              id: resourceId('Microsoft.Network/frontDoors/frontEndEndpoints', frontDoorName, frontDoorApiFrontedName)
            }            
            {
              id: resourceId('Microsoft.Network/frontDoors/frontEndEndpoints', frontDoorName, frontDoorLogsFrontedName)
            }
            {
              id: resourceId('Microsoft.Network/frontDoors/frontEndEndpoints', frontDoorName, frontDoorLogsIngestionFrontedName)
            }
          ]
          acceptedProtocols: [
            'Https'
          ]
          patternsToMatch: [
            '/*'
          ]
          routeConfiguration: {
            '@odata.type': '#Microsoft.Azure.FrontDoor.Models.FrontdoorForwardingConfiguration'
            forwardingProtocol: 'HttpOnly'
            backendPool: {
              id: resourceId('Microsoft.Network/frontDoors/backEndPools', frontDoorName, backendPoolName)
            }
          }
          enabledState: 'Enabled'
        }
      }
      {
        name: ruleHttpsTohttp
        properties: {
          frontendEndpoints: [
            {
              id: resourceId('Microsoft.Network/frontDoors/frontEndEndpoints', frontDoorName, frontDoorBaseFrontedName)
            }           
            {
              id: resourceId('Microsoft.Network/frontDoors/frontEndEndpoints', frontDoorName, frontDoorApiFrontedName)
            }            
            {
              id: resourceId('Microsoft.Network/frontDoors/frontEndEndpoints', frontDoorName, frontDoorLogsFrontedName)
            }
            {
              id: resourceId('Microsoft.Network/frontDoors/frontEndEndpoints', frontDoorName, frontDoorLogsIngestionFrontedName)
            }
          ]
          acceptedProtocols: [
            'Http'
          ]
          patternsToMatch: [
            '/*'
          ]
          routeConfiguration: {
            '@odata.type': '#Microsoft.Azure.FrontDoor.Models.FrontdoorRedirectConfiguration'
            redirectProtocol: 'HttpsOnly'
            redirectType: 'Moved'
          }
          enabledState: 'Enabled'
        }
      }
    ]
  }
}

output id string = frontDoor.id
