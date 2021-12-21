Fix docker build not showing any output from commands:
```
export DOCKER_BUILDKIT=0
```

Run docker-compose
```
docker-compose -f "docker-compose-agent.yml" up -d --build
```


For Development:
Run installer Visual Studio Community
Choose platform ASP .Net and .Net. 
Choose .NET SDK 4.6.1
