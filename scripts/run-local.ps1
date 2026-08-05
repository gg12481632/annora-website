[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$apiPath = Join-Path $repositoryRoot "src\Annora.Api"
$webPath = Join-Path $repositoryRoot "src\Annora.Web"

Write-Host "Annora local environment" -ForegroundColor Cyan
Write-Host "Repository: $repositoryRoot"

function Test-Docker {
    $process = Start-Process `
        -FilePath "docker.exe" `
        -ArgumentList "info" `
        -NoNewWindow `
        -Wait `
        -PassThru `
        -RedirectStandardOutput "$env:TEMP\annora-docker-output.txt" `
        -RedirectStandardError "$env:TEMP\annora-docker-error.txt"

    if ($process.ExitCode -ne 0) {
        $errorText = Get-Content `
            "$env:TEMP\annora-docker-error.txt" `
            -ErrorAction SilentlyContinue

        throw "Docker Desktop is not running. $errorText"
    }
}

function Start-Azurite {
    $container = docker ps -a `
        --filter "name=^/azurite$" `
        --format "{{.Names}}"

    if ($container -eq "azurite") {
        $running = docker ps `
            --filter "name=^/azurite$" `
            --format "{{.Names}}"

        if ($running -ne "azurite") {
            Write-Host "Starting existing Azurite container..."
            docker start azurite | Out-Null
        }
        else {
            Write-Host "Azurite is already running."
        }

        return
    }

    $dataPath = "C:\Research\AzuriteData"

    New-Item `
        -ItemType Directory `
        -Path $dataPath `
        -Force |
        Out-Null

    Write-Host "Creating Azurite container..."

    docker run `
        --name azurite `
        --detach `
        --restart unless-stopped `
        --publish 10000:10000 `
        --publish 10001:10001 `
        --publish 10002:10002 `
        --volume "${dataPath}:/data" `
        mcr.microsoft.com/azure-storage/azurite `
        azurite `
        --blobHost 0.0.0.0 `
        --queueHost 0.0.0.0 `
        --tableHost 0.0.0.0 `
        --location /data |
        Out-Null
}

Test-Docker
Start-Azurite

Write-Host ""
Write-Host "Starting Annora API..." -ForegroundColor Green

Start-Process powershell `
    -WorkingDirectory $apiPath `
    -ArgumentList @(
        "-NoExit",
        "-Command",
        "dotnet run"
    )

Write-Host "Starting static web server..." -ForegroundColor Green

Start-Process powershell `
    -WorkingDirectory $webPath `
    -ArgumentList @(
        "-NoExit",
        "-Command",
        "python -m http.server 8081 --bind 127.0.0.1"
    )

Start-Sleep -Seconds 3

Start-Process "http://localhost:8081/create.html"

Write-Host ""
Write-Host "Frontend: http://localhost:8081" -ForegroundColor Cyan
Write-Host "API:      http://localhost:7071/api" -ForegroundColor Cyan
Write-Host "Azurite:  ports 10000-10002" -ForegroundColor Cyan