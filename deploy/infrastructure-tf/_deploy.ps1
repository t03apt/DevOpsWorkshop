terraform init

terraform -v

$subscription = '83618f74-b58e-4b3c-a980-5f7796eb3460'
az ad sp create-for-rbac --role="Contributor" --scopes="/subscriptions/83618f74-b58e-4b3c-a980-5f7796eb3460"

$Env:ARM_CLIENT_ID = "89be6b9d-0356-4296-b4b2-6dae1eaeabca"
$Env:ARM_CLIENT_SECRET = "EIS8Q~1QAF_MayMxOiQmcuRNgFStlw4aVoXO6da6"
$Env:ARM_SUBSCRIPTION_ID = "83618f74-b58e-4b3c-a980-5f7796eb3460"
$Env:ARM_TENANT_ID = "f6508074-0ecc-41a8-9763-48cba4d10c29"

terraform plan
terraform apply