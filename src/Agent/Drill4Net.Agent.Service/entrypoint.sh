#!/bin/bash

echo MESSAGE_SERVER_ADDRESS:
echo ${MESSAGE_SERVER_ADDRESS}

if [[ -z "${MESSAGE_SERVER_ADDRESS}" ]]; 
then
  echo "Value MESSAGE_SERVER_ADDRESS is undefined"
  exit 1;
fi

echo 'ENV:'
env

envsubst < svc.yml.template > svc.yml

echo 'cat svc.yml'

cat svc.yml

#dotnet Drill4Net.Agent.Service.dll
