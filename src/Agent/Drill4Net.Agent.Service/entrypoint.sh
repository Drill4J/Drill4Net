#!/bin/bash
echo KAFKA_SERVER_ADDRESS:
echo ${KAFKA_SERVER_ADDRESS}

if [[ -z "${KAFKA_SERVER_ADDRESS}" ]]; 
then
  echo "Value KAFKA_SERVER_ADDRESS is undefined"
  exit 1;
fi

ls -la
pwd

envsubst < svc.yml.template > svc.yml

echo 'cat svc.yml'

cat svc.yml

#dotnet Drill4Net.Agent.Service.dll
