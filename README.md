
For Development:

Use Visual Studio Community 2022 because:

```
C:\Program Files\dotnet\sdk\6.0.101\Sdks\Microsoft.NET.Sdk\targets\Microsoft.NET.Sdk.DefaultItems.targets(134,5): warning NETSDK1182: Targeting .NET 6.0 in Visual Studio 2019 is not supported.
```



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

Run docker-compose with admin and kafka in PowerShell or git-bash

```
docker-compose up -d
```

Start Visual Studio


Change svc.yml to:

```
...
Type: agent-service # type as hint
Servers: # list of Kafka servers
   #- 'localhost:9093' # from host
   - 'host.docker.internal:9093' # from Docker
...
```

Enter to Drill4Net.sln

Press Build

Choose Drill4Net.Agent.Service

Choose Docker

Press Docker

 
