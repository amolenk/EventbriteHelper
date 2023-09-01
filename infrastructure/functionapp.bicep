param functionAppName string
param storageAccountName string
param location string = resourceGroup().location


var storageContributorRole = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '86e8f5dc-a6e9-4c67-9d15-de283e8eac25')

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}

resource appServicePlan 'Microsoft.Web/serverfarms@2020-12-01' = {
  name: 'appServicePlan'
  location: location
  kind: 'linux'        // required for using linux
  sku: {
    name: 'Y1'
    capacity: 1
  }
  properties: {
    reserved: true     // required for using linux
  }
}

resource functionApp 'Microsoft.Web/sites@2022-09-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'AzureWebJobsStorage__accountName'
          value: storageAccount.name
        }
      ]
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
    }
    httpsOnly: true
  }
}

resource roleAssigment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, storageContributorRole, functionApp.name)
  scope: storageAccount
  properties: {
    roleDefinitionId: storageContributorRole // storage account contributor
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}
