Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Invoke-WebRequest https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.ps1 -OutFile dotnet-install.ps1
    & powershell -ExecutionPolicy Bypass -File ./dotnet-install.ps1 -Version 8.0.413 -InstallDir "$PSScriptRoot/.dotnet"
    $env:PATH = "$PSScriptRoot/.dotnet;" + $env:PATH
}

msbuild SpawnEditor.sln /t:Restore,Build /p:Configuration=Release
