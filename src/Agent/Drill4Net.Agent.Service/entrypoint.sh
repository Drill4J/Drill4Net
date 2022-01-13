#!/bin/bash

if [[ -z "${KAFKA_SERVER_PORT}" ]]; 
then
  echo "Value KAFKA_SERVER_PORT is undefined"
  exit 1;
fi


envsubst < svc.yml.tamplate > svc.yml

dotnet /app/Drill4Net.Agent.Service.dll
