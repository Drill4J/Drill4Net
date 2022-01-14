#!/bin/bash
echo KAFKA_SERVER_PORT:
echo ${KAFKA_SERVER_PORT}

if [[ -z "${KAFKA_SERVER_PORT}" ]]; 
then
  echo "Value KAFKA_SERVER_PORT is undefined"
  exit 1;
fi

ls -la
pwd

envsubst < svc.yml.tamplate > svc.yml

echo 'cat svc.yml'

cat svc.yml

#dotnet Drill4Net.Agent.Service.dll
