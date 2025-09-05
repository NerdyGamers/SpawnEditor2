#!/usr/bin/env bash
#
# setup.sh – install the ASP.NET Core SDK 8.0 on Ubuntu 24.04
# Usage:   bash setup.sh
# The script is safe to re-run (it exits early if the SDK is present).

set -euo pipefail

SDK_VERSION="8.0"
PACKAGE="dotnet-sdk-${SDK_VERSION}"

if dotnet --list-sdks 2>/dev/null | grep -q "^${SDK_VERSION}\."; then
  echo "✅ .NET SDK ${SDK_VERSION} is already installed – nothing to do."
  exit 0
fi

echo "🔍 Updating package index & installing prerequisites…"
sudo apt-get update -y
sudo apt-get install -y wget apt-transport-https software-properties-common

echo "🔑 Adding Microsoft package feed for Ubuntu 24.04…"
wget -qO /tmp/packages-microsoft-prod.deb \
     https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb
sudo dpkg -i /tmp/packages-microsoft-prod.deb
rm /tmp/packages-microsoft-prod.deb

echo "📦 Installing .NET SDK ${SDK_VERSION} (includes ASP.NET Core)…"
sudo apt-get update -y
sudo apt-get install -y "${PACKAGE}"

echo "🎉 Installation complete. Installed SDKs:"
dotnet --list-sdks

echo "⚙️ Restoring NuGet packages..."
dotnet restore
