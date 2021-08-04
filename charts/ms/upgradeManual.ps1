param(
    [switch] $dryRun = $true,
    [string] $acr='atacrdev.azurecr.io',
    [string] $imageTag='20210705061138-23',
    [string] $inputIP='138.91.117.141',
    [string] $apiUrl='api2.007exs.work',
    [string] $apiPath='/',
    [string] $logsUrl='logs2.007exs.work',
    [string] $logsIngestionUrl='ingestion.logs2.007exs.work',
    [string] $groupName='at-dev-ms2',
    [string] $seqDiskName='at-dev-ms2-aks-seq-disk',
    [string] $seqDiskURI='/subscriptions/d60dfcd2-2dfd-43a3-8f4c-6b80c455bc88/resourceGroups/at-dev-ms2/providers/Microsoft.Compute/disks/at-dev-ms2-aks-seq-disk'
)

$scriptPath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptPath

Push-Location -Path $dir

if($dryRun) {
    helm upgrade --install ms . --set ms-api-customers.image.repository=$acr `
    --set ms-api-customers.image.tag=$imageTag `
    --set ingress-nginx.controller.service.loadBalancerIP=$inputIP `
    --set ms-api-customers.ingress.hosts[0].host=$apiUrl `
    --set ms-api-customers.ingress.hosts[0].paths[0].path=$apiPath `
    --set seq.ui.ingress.hosts[0]=$logsUrl `
    --set seq.ingestion.ingress.hosts[0]=$logsIngestionUrl `
    --set ingress-nginx.controller.service.annotations."service\.beta\.kubernetes\.io/azure-load-balancer-resource-group"=$groupName `
    --set disks.seq.azureDisk.diskName=$seqDiskName `
    --set disks.seq.azureDisk.diskURI=$seqDiskURI `
    --debug `
    --dry-run
} else{
    helm upgrade --install ms . --set ms-api-customers.image.repository=$acr `
    --set ms-api-customers.image.tag=$imageTag `
    --set ingress-nginx.controller.service.loadBalancerIP=$inputIP `
    --set ms-api-customers.ingress.hosts[0].host=$apiUrl `
    --set ms-api-customers.ingress.hosts[0].paths[0].path=$apiPath `
    --set seq.ui.ingress.hosts[0]=$logsUrl `
    --set seq.ingestion.ingress.hosts[0]=$logsIngestionUrl `
    --set ingress-nginx.controller.service.annotations."service\.beta\.kubernetes\.io/azure-load-balancer-resource-group"=$groupName `
    --set disks.seq.azureDisk.diskName=$seqDiskName `
    --set disks.seq.azureDisk.diskURI=$seqDiskURI
}

Pop-Location
