$tempRepository = "$PSScriptRoot/../../GitWorkshopTempRepository"

if (Test-Path $tempRepository) {
    Remove-Item $tempRepository -Force -Recurse | Out-Null
}
New-Item -ItemType Directory -Force -Path $tempRepository | Out-Null
Set-Location -Path $tempRepository

git init
git branch -m main