Set-StrictMode -Version Latest

$privateScripts = Get-ChildItem `
    -Path (Join-Path $PSScriptRoot "Private") `
    -Filter "*.ps1" `
    -File

foreach ($scriptFile in $privateScripts) {
    . $scriptFile.FullName
}

$publicScripts = Get-ChildItem `
    -Path (Join-Path $PSScriptRoot "Public") `
    -Filter "*.ps1" `
    -File

foreach ($scriptFile in $publicScripts) {
    . $scriptFile.FullName
}

Export-ModuleMember -Function Annora