param subDomainApi string
param subDomainLogs string
param frontDoorId string
param dnsZoneName string

resource dnsZone 'Microsoft.Network/dnsZones@2018-05-01' existing = {
  name: dnsZoneName
}

resource dnsZoneApi 'Microsoft.Network/dnsZones/CNAME@2018-05-01' = {
  parent: dnsZone
  name: subDomainApi
  properties: {
    TTL: 60
    targetResource: {
      id: frontDoorId
    }
  }
}

resource dnsZoneApiIngestionLogs 'Microsoft.Network/dnszones/CNAME@2018-05-01' = {
  parent: dnsZone
  name: 'ingestion.${subDomainLogs}'
  properties: {
    TTL: 60
    targetResource: {
      id: frontDoorId
    }
  }
}

resource dnsZoneApiLogs 'Microsoft.Network/dnszones/CNAME@2018-05-01' = {
  parent: dnsZone
  name: subDomainLogs
  properties: {
    TTL: 60
    targetResource: {
      id: frontDoorId
    }
  }
}
