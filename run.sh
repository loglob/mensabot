#!/bin/sh
cd "$(dirname "$(realpath $0)")"
mkdir -p ./logs
dotnet run 2>&1 | tee `date +./logs/mensabot_%F_%X.log`
