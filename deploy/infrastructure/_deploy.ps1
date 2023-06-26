az upgrade
az bicep upgrade

$subscription = '83618f74-b58e-4b3c-a980-5f7796eb3460'

az account set --subscription $subscription

az deployment sub create --name deployment --template-file '.\main.bicep' --location 'west europe'