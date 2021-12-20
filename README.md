Fix docker build not showing any output from commands:
```
export DOCKER_BUILDKIT=0
```

Run docker-compose
```
docker-compose -f "docker-compose-agent.yml" up -d --build
```
