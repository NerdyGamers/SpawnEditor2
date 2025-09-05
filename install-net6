# Script to install .NET SDK 9.0 on Ubuntu 24.04 (which is what OpenAI Codex is running on)
# https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual?WT.mc_id=dotnet-35129-website#scripted-install

DOTNET_VERSION=9.0.204

# Install .NET Dependencies
sudo apt-get update
sudo apt install -y zlib1g ca-certificates libc6 libgcc-s1 libicu74 libssl3 libstdc++6 libunwind8 zlib1g

# Download and run the .NET SDK installer script
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x ./dotnet-install.sh
./dotnet-install.sh --version $DOTNET_VERSION
rm -f dotnet-install.sh

# Set $PATH
DOTNET_ROOT=$HOME/.dotnet
BASHRC_FILE="/root/.bashrc"
DOTNET_PATH_EXPORT_LINE="export PATH=\"$DOTNET_ROOT:$DOTNET_ROOT/tools:\$PATH\""
echo "$DOTNET_PATH_EXPORT_LINE" >> "$BASHRC_FILE"
export PATH=$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH
