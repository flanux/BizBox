#!/usr/bin/env bash
# Bizbox setup script for Mac/Linux/Codespaces.
# Run this once after cloning: ./setup.sh
set -e

echo "Restoring packages..."
dotnet restore

if ! command -v dotnet-ef &> /dev/null; then
    echo "Installing dotnet-ef tool..."
    dotnet tool install --global dotnet-ef
    export PATH="$PATH:$HOME/.dotnet/tools"
fi

echo "Building..."
dotnet build

if [ ! -d "Migrations" ]; then
    echo "No migrations found — creating initial migration..."
    dotnet ef migrations add InitialCreate
fi

# After changing EF Core models, create a named migration explicitly, e.g.
# dotnet ef migrations add AddManufacturerAndSpecs
# dotnet ef database update

echo "Applying database migrations..."
dotnet ef database update

echo ""
echo "Setup complete. Run 'dotnet run' to start the app."
echo "Demo admin login: admin@bizbox.com / Admin@123"
