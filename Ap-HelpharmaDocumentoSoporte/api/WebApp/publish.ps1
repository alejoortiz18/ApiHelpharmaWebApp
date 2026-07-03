#Requires -Version 5.1

param(
    [string]$Configuration = "Release",
    [string]$PublishPath = "C:\inetpub\ServiceAPI",
    [string]$ProjectPath = "$PSScriptRoot\WebApp\WebApp.csproj",
    [string]$AppPoolName = "Api-Soportes",
    [switch]$SkipAppPoolRestart
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Import-IisModule {
    if (-not (Get-Module -ListAvailable -Name WebAdministration)) {
        throw "El modulo WebAdministration no esta disponible. Ejecute el script como Administrador."
    }

    Import-Module WebAdministration -ErrorAction Stop
}

function Stop-AppPoolIfConfigured {
    if ($SkipAppPoolRestart -or [string]::IsNullOrWhiteSpace($AppPoolName)) {
        return
    }

    Import-IisModule

    if (Test-Path "IIS:\AppPools\$AppPoolName") {
        Write-Step "Deteniendo App Pool '$AppPoolName'"
        if ((Get-WebAppPoolState -Name $AppPoolName).Value -ne "Stopped") {
            Stop-WebAppPool -Name $AppPoolName -ErrorAction Stop
            Start-Sleep -Seconds 3
        }
    }
    else {
        Write-Host "App Pool '$AppPoolName' no encontrado. Se omite detencion." -ForegroundColor Yellow
    }
}

function Start-AppPoolIfConfigured {
    if ($SkipAppPoolRestart -or [string]::IsNullOrWhiteSpace($AppPoolName)) {
        return
    }

    Import-IisModule

    if (Test-Path "IIS:\AppPools\$AppPoolName") {
        Write-Step "Iniciando App Pool '$AppPoolName'"
        if ((Get-WebAppPoolState -Name $AppPoolName).Value -ne "Started") {
            Start-WebAppPool -Name $AppPoolName -ErrorAction Stop
        }
    }
}

function Copy-PublishOutput {
    param(
        [string]$Source,
        [string]$Destination
    )

    Write-Step "Copiando archivos publicados a IIS"
    robocopy $Source $Destination /MIR /NFL /NDL /NJH /NJS /NP /R:3 /W:2 | Out-Null

    if ($LASTEXITCODE -ge 8) {
        throw "robocopy finalizo con codigo $LASTEXITCODE"
    }
}

try {
    Write-Step "Verificando herramientas"
    $dotnetVersion = dotnet --version
    Write-Host ".NET SDK: $dotnetVersion"

    if (-not (Test-Path $ProjectPath)) {
        throw "No se encontro el proyecto: $ProjectPath"
    }

    if (-not (Test-Path $PublishPath)) {
        Write-Step "Creando carpeta de publicacion"
        New-Item -ItemType Directory -Path $PublishPath -Force | Out-Null
    }

    $stagingPath = Join-Path $env:TEMP "ServiceAPI-publish-$([Guid]::NewGuid().ToString('N'))"
    New-Item -ItemType Directory -Path $stagingPath -Force | Out-Null

    $appSettingsTarget = Join-Path $PublishPath "appsettings.json"
    $appSettingsBackup = Join-Path $stagingPath "appsettings.production.backup.json"

    if (Test-Path $appSettingsTarget) {
        Write-Step "Respaldando appsettings.json existente"
        Copy-Item -Path $appSettingsTarget -Destination $appSettingsBackup -Force
    }

    Write-Step "Compilando y publicando aplicacion"
    Write-Host "Proyecto: $ProjectPath"
    Write-Host "Staging:  $stagingPath"
    Write-Host "Destino:  $PublishPath"
    Write-Host "Config:   $Configuration"

    dotnet publish $ProjectPath `
        --configuration $Configuration `
        --output $stagingPath `
        --no-self-contained `
        /p:EnvironmentName=Production

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish finalizo con codigo $LASTEXITCODE"
    }

    if (Test-Path $appSettingsBackup) {
        Write-Step "Restaurando appsettings.json de produccion"
        Copy-Item -Path $appSettingsBackup -Destination (Join-Path $stagingPath "appsettings.json") -Force
    }

    Stop-AppPoolIfConfigured
    Copy-PublishOutput -Source $stagingPath -Destination $PublishPath
    Start-AppPoolIfConfigured

    if (Test-Path $stagingPath) {
        Remove-Item -Path $stagingPath -Recurse -Force
    }

    Write-Step "Publicacion completada"
    Write-Host "Ruta IIS: $PublishPath" -ForegroundColor Green
    if (-not $SkipAppPoolRestart -and -not [string]::IsNullOrWhiteSpace($AppPoolName)) {
        Write-Host "App Pool: $AppPoolName" -ForegroundColor Green
    }
}
catch {
    Write-Host ""
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
