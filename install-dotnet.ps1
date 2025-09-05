#!/usr/bin/env bash
#
# install-dotnet.ps1 – install .NET SDK 9.0 using the official dotnet-install script

set -euo pipefail

if dotnet --list-sdks 2>/dev/null | grep -q "^9\.0"; then
  echo "✅ .NET SDK 9.0 is already installed – nothing to do."
  exit 0
fi

echo "📦 Installing .NET SDK 9.0…"
curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 9.0 --install-dir /root/.dotnet >/tmp/dotnet-install.log && tail -n 20 /tmp/dotnet-install.log
rm dotnet-install.sh

export PATH=/root/.dotnet:$PATH

echo "🎉 Installation complete. Installed SDKs:"
dotnet --list-sdks

echo "⚙️ Restoring NuGet packages..."
dotnet restore

