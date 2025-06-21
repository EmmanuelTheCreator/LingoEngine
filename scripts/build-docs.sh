#!/usr/bin/env bash
set -e
export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"

docfx docs/docfx/docfx.json
