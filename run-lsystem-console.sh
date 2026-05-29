#!/bin/bash
# L-System Console Test Application launcher
# Usage: ./run-lsystem-console.sh

cd "$(dirname "$0")" || exit 1

if [ ! -f "LSystemConsoleApp.csproj" ]; then
    echo "❌ Error: LSystemConsoleApp.csproj not found"
    echo "Please run this script from the Arboricultor project root directory"
    exit 1
fi

echo "🌱 L-System Console Test Application"
echo "===================================="

if [ "$1" = "--build" ] || [ "$1" = "-b" ]; then
    echo "Building project..."
    dotnet build LSystemConsoleApp.csproj -c Release
    if [ $? -ne 0 ]; then
        echo "❌ Build failed"
        exit 1
    fi
fi

if [ -f "bin/Release/net10.0/LSystemConsoleApp.dll" ]; then
    echo "Running from compiled binary..."
    dotnet bin/Release/net10.0/LSystemConsoleApp.dll
else
    echo "Running from source (dotnet run)..."
    dotnet run --project LSystemConsoleApp.csproj
fi
