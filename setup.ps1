# Bizbox setup script for Windows PowerShell.
# Run this once after cloning: .\setup.ps1
$ErrorActionPreference = "Stop"

Write-Host "Restoring packages..."
dotnet restore

if (-not (Get-Command dotnet-ef -ErrorAction SilentlyContinue)) {
    Write-Host "Installing dotnet-ef tool..."
    dotnet tool install --global dotnet-ef
}

Write-Host "Building..."
dotnet build

if (-not (Test-Path "Migrations")) {
    Write-Host "No migrations found - creating initial migration..."
    dotnet ef migrations add InitialCreate
}

Write-Host "Applying database migrations..."
dotnet ef database update

Write-Host ""
Write-Host "Setup complete. Run 'dotnet run' to start the app."
Write-Host "Demo admin login: admin@bizbox.com / Admin@123"
