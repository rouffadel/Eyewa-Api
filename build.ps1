# PowerShell Build & Package Script for Eyewa.Api
# This script cleans, builds, publishes, and packages the API into a zip file for deployment to IIS or VPS.

$ErrorActionPreference = "Stop"

# Configuration variables
$ProjectName = "Eyewa.Api"
$ProjectPath = "./Eyewa.Api/Eyewa.Api.csproj"
$BuildConfiguration = "Release"
$OutputFolder = "./publish-release"
$ZipFileName = "Eyewa-Api-Release.zip"

Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "Starting Build & Package process for $ProjectName" -ForegroundColor Cyan
Write-Host "Configuration: $BuildConfiguration" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# 1. Clean previous build outputs
if (Test-Path $OutputFolder) {
    Write-Host "[1/5] Cleaning existing publish directory: $OutputFolder..." -ForegroundColor Yellow
    Remove-Item -Path $OutputFolder -Recurse -Force
}
if (Test-Path $ZipFileName) {
    Write-Host "[1/5] Removing existing zip archive: $ZipFileName..." -ForegroundColor Yellow
    Remove-Item -Path $ZipFileName -Force
}

# 2. Restore NuGet packages
Write-Host "[2/5] Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore

# 3. Build the solution
Write-Host "[3/5] Compiling project under $BuildConfiguration configuration..." -ForegroundColor Yellow
dotnet build --configuration $BuildConfiguration --no-restore

# 4. Publish Web API
Write-Host "[4/5] Publishing $ProjectName to folder: $OutputFolder..." -ForegroundColor Yellow
dotnet publish $ProjectPath --configuration $BuildConfiguration --output $OutputFolder --no-build

# 5. Create ZIP Archive
Write-Host "[5/5] Packaging published files into $ZipFileName..." -ForegroundColor Yellow
Compress-Archive -Path "$OutputFolder\*" -DestinationPath $ZipFileName -Force

Write-Host ""
Write-Host "=============================================" -ForegroundColor Green
Write-Host "Build & Package Complete!" -ForegroundColor Green
Write-Host "Archive Location: $PSScriptRoot\$ZipFileName" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
