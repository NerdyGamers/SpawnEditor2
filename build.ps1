# Script to install .NET SDK 9.0 on Ubuntu 24.04 (which is what OpenAI Codex is running on)
# https://learn.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual?WT.mc_id=dotnet-35129-website#scripted-install

# Download and run the .NET SDK installer script
curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 9.0 --install-dir /root/.dotnet >/tmp/dotnet-install.log && tail -n 20 /tmp/dotnet-install.log
rm -f dotnet-install.sh

# Set $PATH
DOTNET_ROOT=$HOME/.dotnet
BASHRC_FILE="/root/.bashrc"
DOTNET_PATH_EXPORT_LINE="export PATH=\"$DOTNET_ROOT:$DOTNET_ROOT/tools:\$PATH\""
echo "$DOTNET_PATH_EXPORT_LINE" >> "$BASHRC_FILE"
export PATH=$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH
