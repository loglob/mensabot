#!/bin/sh

LOGFILE=`date +./logs/mensabot_%F_%X.log`

cd "$(dirname "$(realpath $0)")"
mkdir -p ./logs
dotnet run 2>&1 | tee "$LOGFILE"

if [ ! -s "$LOGFILE" ]
then
	rm "$LOGFILE"
fi
