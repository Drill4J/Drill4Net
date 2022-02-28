## Dotnet publish Drill4NET to local directory
Install Git, Dotnet SDK for 5.0, 6.0
Run PowerShell script Drill4NetBuildOnWindows.ps1

## Deploy Drill4Net local application from docker to Windows
Download https://github.com/epamX/Drill4Net/blob/main/docker-compose-run-distribution.yml to empty folder.
```
docker-compose -f docker-compose-run-distribution.yml up
```
Agree for share/mount volume in docker for windows

Script in Container have been download binary to distribution folder. After work this directory contains:
```
Drill4Net.Agent.Plugins.NUnit
Drill4Net.Agent.Plugins.SpecFlow
Drill4Net.Agent.Plugins.xUnit
Drill4Net.Agent.TestRunner
Drill4Net.Agent.Transmitter
Drill4Net.Configurator.App
Drill4Net.Injector.App
```


## Start Drill4Net.Agent.Service in Docker and connect to Kafka in Docker
Run docker-compose in root project directory with admin and kafka in PowerShell or git-bash

```
docker-compose up -d
```

Enter to Docker container and run
```
docker exec -it drill4net-service bash
dotnet Drill4Net.Agent.Worker.dll --cfg_path=svc.yml --target_session=4fbf1ce9-c51d-4b35-a2cf-52f6c07d8932 --target_name=IHS-bdd --target_version=0.1.0
```

## Start Kafka in Docker. Later start Drill4Net.Agent.Service in Docker and connect to Kafka

```
docker-compose -f docker-compose-admin-without-agent.yml up -d
```

Wait for start Kafka. Pass environment variable to docker in Git-bash (Windows) for ghcr.io/epamx/drill4net:latest
```
winpty docker run -it -e MESSAGE_SERVER_ADDRESS='host.docker.internal:9093' -e DRILL_ADMIN_ADDRESS='drill-admin:8090' --network=drill4net-dev-network ghcr.io/epamx/drill4net:latest
```

## Build Drill4Net.Agent.Service in Docker and connect to Kafka in Docker
Use Visual Studio Community 2022 because Targeting .NET 6.0 in Visual Studio 2019 is not supported.

Run installer Visual Studio Community 2022 - https://docs.microsoft.com/ru-ru/visualstudio/releases/2022/release-notes

Choose workload:

1) ASP .Net and web development
2) .Net desktop development

Choose individual component (Add components)

.NET:

- .NET 5.0 Runtime
- .NET 6.0 Runtime
- .NET Core 2.1 Runtime
- .NET Core 3.1 Runtime
- .NET Framework 4.6.1 SDK
- .NET Framework 4.6.1 targeting pack
- .NET Framework 4.6.2 SDK
- .NET Framework 4.6.2 targeting pack
- .NET Framework 4.7 SDK
- .NET Framework 4.7 targeting pack
- .NET Framework 4.7.1 SDK
- .NET Framework 4.7.1 targeting pack
- .NET Framework 4.7.2 SDK
- .NET Framework 4.7.2 targeting pack
- .NET Framework 4.8 SDK
- .NET Framework 4.8 targeting pack
- .NET SDK
- .NET WebAssembly build tools
- Advanced ASP.NET features
- Development Tools for .NET Core 2.1
- Web development tools for .NET Core 2.1

Cloud, database and server:

- Container development tools

Code tools:

- Class designer
- Dependency Validation
- Developer Analutics tools
- DGML editor
- Git for Windows
- Nuget package manager
- Nuget target and build tasks
- Text Template Transformation

Compilers, build tools, and runtimes:

- .NET Compiler Platform SDK
- C# and Visual Basic Roslyn compilers
- MSBuild

Debuging and testing:

- .NET Debugging with WSL
- .NET profiling tools

Development activities:

- ASP.NET and web development prerequisites
- C# and Visual Basic
- F# desktop language support
- F# language support
- F# language support for web projects
- IntelliCode

Press "Not now, may be later"
Clone this repository.

Configuring NuGet Package Sources:

In Visual Studio, go to Tools > Options and then select Package Sources under the NuGet Package Manager.

Choose the Add icon (+), edit the Name - nuget.org, and then https://api.nuget.org/v3/index.json in the Source Click Update after updating the feed link.

Close Visual Studio

Install and start [Docker Desktop](https://www.docker.com/products/docker-desktop)

Run docker-compose in root project directory with admin and kafka in PowerShell or git-bash

```
docker-compose up -d
```

Start Visual Studio


Change src\Agent\Drill4Net.Agent.Service\svc.yml to:

```
...
Type: agent-service # type as hint
Servers: # list of Kafka servers
   #- 'localhost:9093' # from host
   - 'host.docker.internal:9093' # from Docker
...
```

Enter to Drill4Net.sln

Wait Ready in status line (below)

Press Build - Build Solution

Press Build - Clean Solution

Press Build - Build Solution

Choose Drill4Net.Agent.Service

Choose and start profile Docker


## Debug Docker build when build without Visual Studio

Use src/Agent/Drill4Net.Agent.Service/Dockerfile-new

Add this line to Directory.Build.props in root project

```
  <PropertyGroup>
	<AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>
	<GenerateBindingRedirectsOutputType>false</GenerateBindingRedirectsOutputType>
	<PublishRepositoryUrl>true</PublishRepositoryUrl>
	<EmbedUntrackedSources>true</EmbedUntrackedSources>
  </PropertyGroup>
```

Add `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to src\Agent\Drill4Net.Agent.Service\Drill4Net.Agent.Service.csproj
```
  <PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Release|AnyCPU'">
    <OutputPath>..\..\..\build\bin\Release\Drill4Net.Agent.Service</OutputPath>
	<GenerateAssemblyInfo>false</GenerateAssemblyInfo>
  </PropertyGroup>
```

Build in Git-bash (Windows)
```
docker-compose -f docker-compose-build.yml build
```

Build in Git-bash (Windows) without cache layers
```
DOCKER_BUILDKIT=0 docker-compose -f docker-compose-build.yml build --no-cache
```

Pass environment variable to docker in Git-bash (Windows) for drill4net_agent:latest
```
winpty docker run -it -e MESSAGE_SERVER_ADDRESS='host.docker.internal:9093' -e DRILL_ADMIN_ADDRESS='drill-admin:8090' --network=drill4net-dev-network drill4net_agent:latest
```

Enter to Docker container and run
```
dotnet Drill4Net.Agent.Worker.dll --cfg_path=svc.yml --target_session=4fbf1ce9-c51d-4b35-a2cf-52f6c07d8932 --target_name=IHS-bdd --target_version=0.1.0
```
