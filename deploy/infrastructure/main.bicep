targetScope = 'subscription'

param environment string = 'dev'
param prefix string = 'devopsworkshop'
param location string = 'west europe'

resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: 'rg-${prefix}-${environment}'
  location: location
}

module storage 'storage.bicep' = {
  scope: resourceGroup
  name: '${deployment().name}-storage'
  params: {
    location: location
    name: 'st${prefix}2${environment}'
  }
}

module sql 'sql-server.bicep' = {
  scope: resourceGroup
  name: '${deployment().name}-sql-server'
  params: {
    location: location
    sqlDatabaseName: 'sqldb-${prefix}-${environment}'
    sqlServerAdministratorLogin: 'rfid-admin'
    sqlServerAdministratorLoginPassword: ';S&EjQFV2[ayCGVp'
    sqlServerName: 'sql-${prefix}-${environment}'
  }
}

module logAnalytics 'loganalytics.bicep' = {
  scope: resourceGroup
  name: '${deployment().name}-logAnalytics'
  params: {
    location: location
    name: 'law-${prefix}-${environment}'
  }
}

module appInsight 'appinsights.bicep' = {
  scope: resourceGroup
  name: '${deployment().name}-appInsight'
  params: {
    location: location
    name: 'ai-${prefix}-${environment}'
    logAnalyticsWorkspaceId: logAnalytics.outputs.logAnalyticsWorkspaceId
  }
}

module web 'web.bicep' = {
  scope: resourceGroup
  name: '${deployment().name}-web'
  params: {
    appServiceName: 'app-${prefix}-${environment}'
    appServicePlanName: 'plan-${prefix}-${environment}'
    location: location
  }
}

module configuration 'app-configuration.bicep' = {
  scope: resourceGroup
  name: '${deployment().name}-configuration'
  params: {
    configStoreName: 'appconfig-${prefix}-${environment}'
    location: location
  }
}
