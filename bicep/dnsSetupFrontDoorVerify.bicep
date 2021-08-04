param subDomainApi string
param subDomainLogs string
param dnsZoneName string
param prefix string

resource dnsZone 'Microsoft.Network/dnsZones@2018-05-01' existing = {
  name: dnsZoneName
}

resource dnsZoneAfdverifyApi 'Microsoft.Network/dnszones/CNAME@2018-05-01' = {
  parent: dnsZone
  name: 'afdverify.${subDomainApi}'
  properties: {
    CNAMERecord: {
      cname: 'afdverify.${prefix}-fd.azurefd.net'
    }
    TTL: 60
    targetResource: {}
  }
}

resource dnsZoneAfdverifyIngestionLogs 'Microsoft.Network/dnszones/CNAME@2018-05-01' = {
  parent: dnsZone
  name: 'afdverify.ingestion.${subDomainLogs}'
  properties: {
    CNAMERecord: {
      cname: 'afdverify.${prefix}-fd.azurefd.net'
    }
    TTL: 60
    targetResource: {}
  }
}

resource dnsZoneAfdverifyLogs 'Microsoft.Network/dnszones/CNAME@2018-05-01' = {
  parent: dnsZone
  name: 'afdverify.${subDomainLogs}'
  properties: {
    CNAMERecord: {
      cname: 'afdverify.${prefix}-fd.azurefd.net'
    }
    TTL: 60
    targetResource: {}
  }
}
