targetScope = 'subscription'

var deploymentRegion = 'westeurope'

resource eventbriteHelperRG 'Microsoft.Resources/resourceGroups@2019-05-10' = {
  name: 'EventbriteHelperRG'
  location: deploymentRegion
}

module storage 'storage.bicep' = {
  name: 'storage'
  scope: eventbriteHelperRG
  params: {
    storageAccountName: 'ebhelperstorage'
    location: eventbriteHelperRG.location
  }
}

module functionApp 'functionapp.bicep' = {
  name: 'functionApp'
  scope: eventbriteHelperRG
  params: {
    functionAppName: 'ebhelperfunctionapp'
    location: eventbriteHelperRG.location
  }
}
