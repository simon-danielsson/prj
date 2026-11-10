#!/usr/bin/env bash

set -xe

mkdir -p $HOME/.local/bin

# put this in your .bashrc permanently if you have not already
export PATH="$HOME/.local/bin:$PATH"

if [[ "$OSTYPE" == "linux-gnu"* ]]; then
        dotnet publish -c Release -r linux-x64 --self-contained true \
            -p:PublishSingleFile=true -o out/linux-x64
        cp ./out/linux-x64/prj $HOME/.local/bin
elif [[ "$OSTYPE" == "darwin"* ]]; then
    if [[ $(uname -m) == 'arm64' ]]; then
        dotnet publish -c Release -r osx-arm64 --self-contained true \
            -p:PublishSingleFile=true -o out/osx-arm64
        cp ./out/osx-arm64/prj $HOME/.local/bin
    else
        dotnet publish -c Release -r osx-x64 --self-contained true \
            -p:PublishSingleFile=true -o out/osx-x64
        cp ./out/osx-x64/prj $HOME/.local/bin
    fi
else
    echo "Error: this program might not be compatible with your OS"
fi


