#!/bin/sh

echo "Running in mode: $APP_MODE"

if [ "$APP_MODE" = "api" ]; then
    echo "Starting REST API..."
    dotnet BlockchainApp.dll --urls=http://0.0.0.0:80
elif [ "$APP_MODE" = "background" ]; then
    echo "Starting Kafka Saver..."
    dotnet BlockchainApp.dll --mode=background
else
    echo "Unknown mode: $APP_MODE"
    exit 1
fi