Set-StrictMode -Version Latest

$script:AnnoraRepositoryRoot = (
    Resolve-Path (
        Join-Path $PSScriptRoot "..\..\.."
    )
).Path

$script:AnnoraSolutionPath = Join-Path `
    $script:AnnoraRepositoryRoot `
    "Annora.slnx"

$script:AnnoraGitHubRepository =
    "gg12481632/annora-website"

$script:AnnoraAzureUrl =
    "https://agreeable-sea-06745e503.7.azurestaticapps.net"

function Test-AnnoraCommand {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string] $Name
    )

    return $null -ne (
        Get-Command $Name -ErrorAction SilentlyContinue
    )
}

function Invoke-AnnoraExternalCommand {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string] $Command,

        [Parameter()]
        [string[]] $Arguments = @()
    )

    & $Command @Arguments

    if ($LASTEXITCODE -ne 0) {
        throw (
            "Kommandoen fejlede med exit code " +
            "${LASTEXITCODE}: $Command " +
            ($Arguments -join " ")
        )
    }
}

function Get-AnnoraLatestRunId {
    [CmdletBinding()]
    param()

    $runId = gh run list `
        --repo $script:AnnoraGitHubRepository `
        --limit 1 `
        --json databaseId `
        --jq ".[0].databaseId"

    if ($LASTEXITCODE -ne 0) {
        throw "Kunne ikke hente seneste GitHub Actions-kørsel."
    }

    if ([string]::IsNullOrWhiteSpace($runId)) {
        throw "Der blev ikke fundet nogen GitHub Actions-kørsel."
    }

    return [long] $runId
}

function Show-AnnoraHelp {
    [CmdletBinding()]
    param()

    @"
Annora CLI

Brug:
  annora <kommando> [argumenter]

Kommandoer:
  help                 Vis denne hjælp
  doctor               Kontrollér udviklingsmiljøet
  build                Byg Annora.slnx
  test                 Kør solutionens tests
  clean                Kør dotnet clean
  status               Vis Git-status
  deploys [antal]      Vis GitHub Actions-kørsler
  watch [run-id]       Følg en deployment
  logs [run-id]        Vis deployment-log
  api                  Kontrollér Azure API'et
  web [side]           Åbn Azure-websitet
  local                Start lokalmiljøet
  stop-azurite         Stop Azurite-containeren
  version              Vis CLI- og værktøjsversioner
  root                 Gå til repository-roden

Eksempler:
  annora build
  annora doctor
  annora deploys 5
  annora watch
  annora web listings
"@ | Write-Host
}

function Invoke-AnnoraDoctor {
    [CmdletBinding()]
    param()

    $checks = @(
        @{
            Name = "Repository"
            Test = { Test-Path $script:AnnoraRepositoryRoot }
            Detail = $script:AnnoraRepositoryRoot
        }
        @{
            Name = "Solution"
            Test = { Test-Path $script:AnnoraSolutionPath }
            Detail = $script:AnnoraSolutionPath
        }
        @{
            Name = "Git"
            Test = { Test-AnnoraCommand "git" }
            Detail = "git"
        }
        @{
            Name = ".NET"
            Test = { Test-AnnoraCommand "dotnet" }
            Detail = "dotnet"
        }
        @{
            Name = "Azure CLI"
            Test = { Test-AnnoraCommand "az" }
            Detail = "az"
        }
        @{
            Name = "GitHub CLI"
            Test = { Test-AnnoraCommand "gh" }
            Detail = "gh"
        }
        @{
            Name = "Docker"
            Test = {
                if (-not (Test-AnnoraCommand "docker")) {
                    return $false
                }

                cmd.exe /c "docker info >nul 2>&1"
                return $LASTEXITCODE -eq 0
            }
            Detail = "Docker Desktop"
        }
        @{
            Name = "Functions Tools"
            Test = { Test-AnnoraCommand "func" }
            Detail = "func"
        }
        @{
            Name = "Azure API"
            Test = {
                try {
                    Invoke-RestMethod `
                        -Uri "$script:AnnoraAzureUrl/api/listings" `
                        -Method Get `
                        -TimeoutSec 20 |
                        Out-Null

                    return $true
                }
                catch {
                    return $false
                }
            }
            Detail = "$script:AnnoraAzureUrl/api/listings"
        }
    )

    $results = foreach ($check in $checks) {
        $succeeded = $false

        try {
            $succeeded = [bool] (& $check.Test)
        }
        catch {
            $succeeded = $false
        }

        [PSCustomObject]@{
            Check  = $check.Name
            Status = if ($succeeded) { "OK" } else { "FAILED" }
            Detail = $check.Detail
        }
    }

    $results | Format-Table -AutoSize

    if ($results.Status -contains "FAILED") {
        throw "En eller flere miljøkontroller fejlede."
    }
}