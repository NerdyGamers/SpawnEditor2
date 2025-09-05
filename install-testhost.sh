#!/usr/bin/env bash
set -euo pipefail

DOTNET_ROOT="$(dirname $(readlink -f $(which dotnet)))"
SDK_VERSION="$(dotnet --version)"
HOST_DIR="$DOTNET_ROOT/sdk/$SDK_VERSION/TestHostNetFramework"

if [ -f "$HOST_DIR/testhost.net48.exe" ]; then
  echo "testhost.net48.exe already present"
  exit 0
fi

TMP_DIR=$(mktemp -d)
trap 'rm -rf "$TMP_DIR"' EXIT
pushd "$TMP_DIR" >/dev/null
curl -sSL https://www.nuget.org/api/v2/package/Microsoft.TestPlatform/17.8.0 -o package.zip
unzip -q package.zip
mkdir -p "$HOST_DIR"
cp -r tools/net462/Common7/IDE/Extensions/TestPlatform/* "$HOST_DIR"
popd >/dev/null

echo "Installed testhost.net48.exe to $HOST_DIR"
