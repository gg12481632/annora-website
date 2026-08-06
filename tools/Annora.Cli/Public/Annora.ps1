function Annora {
    [CmdletBinding()]
    param(
        [Parameter(Position = 0)]
        [string] $Command = "help",

        [Parameter(
            Position = 1,
            ValueFromRemainingArguments = $true
        )]
        [string[]] $Arguments = @()        
    )

    $commandName = $Command.ToLowerInvariant()

    switch ($commandName) {
        "help" {
            Show-AnnoraHelp
        }

        "root" {
            Set-Location $script:AnnoraRepositoryRoot
        }

        "build" {
            Push-Location $script:AnnoraRepositoryRoot

            try {
                Invoke-AnnoraExternalCommand `
                    -Command "dotnet" `
                    -Arguments @(
                        "build",
                        $script:AnnoraSolutionPath
                    )
            }
            finally {
                Pop-Location
            }
        }

        "test" {
            Push-Location $script:AnnoraRepositoryRoot

            try {
                Invoke-AnnoraExternalCommand `
                    -Command "dotnet" `
                    -Arguments @(
                        "test",
                        $script:AnnoraSolutionPath
                    )
            }
            finally {
                Pop-Location
            }
        }

        "clean" {
            Push-Location $script:AnnoraRepositoryRoot

            try {
                Invoke-AnnoraExternalCommand `
                    -Command "dotnet" `
                    -Arguments @(
                        "clean",
                        $script:AnnoraSolutionPath
                    )
            }
            finally {
                Pop-Location
            }
        }

        "status" {
            Push-Location $script:AnnoraRepositoryRoot

            try {
                git status
            }
            finally {
                Pop-Location
            }
        }

        "doctor" {
            Invoke-AnnoraDoctor
        }

        "deploys" {
            $limit = 10

            if ($Arguments.Count -gt 0) {
                $limit = [int] $Arguments[0]
            }

            gh run list `
                --repo $script:AnnoraGitHubRepository `
                --limit $limit
        }

        "watch" {
            $runId = if ($Arguments.Count -gt 0) {
                [long] $Arguments[0]
            }
            else {
                Get-AnnoraLatestRunId
            }

            gh run watch $runId `
                --repo $script:AnnoraGitHubRepository
        }

        "logs" {
            $runId = if ($Arguments.Count -gt 0) {
                [long] $Arguments[0]
            }
            else {
                Get-AnnoraLatestRunId
            }

            gh run view $runId `
                --repo $script:AnnoraGitHubRepository `
                --log
        }

        "api" {
            $uri = "$script:AnnoraAzureUrl/api/listings"

            try {
                $listings = Invoke-RestMethod `
                    -Uri $uri `
                    -Method Get `
                    -TimeoutSec 30

                [PSCustomObject]@{
                    Status       = "Available"
                    Uri          = $uri
                    ListingCount = @($listings).Count
                }
            }
            catch {
                [PSCustomObject]@{
                    Status       = "Unavailable"
                    Uri          = $uri
                    ListingCount = $null
                    Error        = $_.Exception.Message
                }
            }
        }

        "web" {
            $page = if ($Arguments.Count -gt 0) {
                $Arguments[0].ToLowerInvariant()
            }
            else {
                "home"
            }

            $url = switch ($page) {
                "home" {
                    $script:AnnoraAzureUrl
                }

                "create" {
                    "$script:AnnoraAzureUrl/create.html"
                }

                "listings" {
                    "$script:AnnoraAzureUrl/listings.html"
                }

                "actions" {
                    "https://github.com/" +
                    "$script:AnnoraGitHubRepository/actions"
                }

                "github" {
                    "https://github.com/" +
                    $script:AnnoraGitHubRepository
                }

                default {
                    throw "Ukendt webside: $page"
                }
            }

            Start-Process $url
        }

        "local" {
            $scriptPath = Join-Path `
                $script:AnnoraRepositoryRoot `
                "scripts\run-local.ps1"

            if (-not (Test-Path $scriptPath)) {
                throw "Scriptet blev ikke fundet: $scriptPath"
            }

            & $scriptPath
        }

        "stop-azurite" {
            $container = docker ps `
                --filter "name=^/azurite$" `
                --format "{{.Names}}"

            if ($container -ne "azurite") {
                Write-Host "Azurite kører ikke."
                return
            }

            docker stop azurite | Out-Null
            Write-Host "Azurite er stoppet."
        }

        "version" {
            $module = Get-Module AnnoraTools

            [PSCustomObject]@{
                AnnoraCli = $module.Version.ToString()
                PowerShell = $PSVersionTable.PSVersion.ToString()
                DotNet = (
                    & dotnet --version 2>$null
                )
                Git = (
                    & git --version 2>$null
                )
                AzureCli = (
                    & az version `
                        --query '"azure-cli"' `
                        --output tsv `
                        2>$null
                )
                GitHubCli = (
                    & gh --version 2>$null |
                    Select-Object -First 1
                )
            } | Format-List
        }

        default {
            Write-Warning "Ukendt Annora-kommando: $Command"
            Show-AnnoraHelp
        }
    }
}