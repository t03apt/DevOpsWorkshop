param name string
param location string

resource storage 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: name
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'

  resource blobServices 'blobServices@2022-09-01' = {
    name: 'default'

    resource blobContainer 'containers@2022-09-01' = {
      name: 'images'
    }
  }
}
