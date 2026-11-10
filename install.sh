#!/usr/bin/env bash

if ! dotnet --info 1>/dev/null ; then
    echo "Error: dotnet is not installed - install it before continuing (see README.md)."
    exit
fi

if [ ! -d "$HOME/.local/bin" ]; then
    mkdir -p $HOME/.local/bin && \
        printf "Info: a new folder '\$HOME/.local/bin' was created.\n" \
        printf "Be sure to add it to your bin path (see README.md).\n"
fi

# put this in your .bashrc permanently if you have not already
export PATH="$HOME/.local/bin:$PATH"

if [[ -z "${EDITOR}" ]]; then
    echo "Info: environment variable \$EDITOR has not yet been set."
    echo "Add this variable to your environment (see README.md)."
fi

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


