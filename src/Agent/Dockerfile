#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
ENV MESSAGE_SERVER_ADDRESS $MESSAGE_SERVER_ADDRESS
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ENV MESSAGE_SERVER_ADDRESS $MESSAGE_SERVER_ADDRESS
WORKDIR /src
COPY . .
RUN ls -la build || true
RUN rm -rf build/bin 
RUN rm -rf build/obj
RUN ls -la build || true
RUN dotnet restore "src/Agent/Drill4Net.Agent.Service/Drill4Net.Agent.Service.csproj"
RUN dotnet restore "src/Agent/Drill4Net.Agent.Worker/Drill4Net.Agent.Worker.csproj"
RUN dotnet restore "src/Configurator/Drill4Net.Configurator.App/Drill4Net.Configurator.App.csproj"
RUN dotnet restore "src/Injector/Drill4Net.Injector.App/Drill4Net.Injector.App.csproj"
RUN dotnet restore "src/Agent/Drill4Net.Agent.TestRunner/Drill4Net.Agent.TestRunner.csproj"
RUN dotnet restore "src/Agent/Drill4Net.Agent.Transmitter/Drill4Net.Agent.Transmitter.csproj"
RUN dotnet restore "src/Agent/Drill4Net.Agent.Plugins.NUnit/Drill4Net.Agent.Plugins.NUnit.csproj"
RUN dotnet restore "src/Agent/Drill4Net.Agent.Plugins.SpecFlow/Drill4Net.Agent.Plugins.SpecFlow.csproj"
RUN dotnet restore "src/Agent/Drill4Net.Agent.Plugins.xUnit/Drill4Net.Agent.Plugins.xUnit.csproj"
WORKDIR "/src/src/Agent/Drill4Net.Agent.Service"
RUN dotnet build "Drill4Net.Agent.Service.csproj" -c Release -o /app/build
WORKDIR "/src/src/Agent/Drill4Net.Agent.Worker"
RUN dotnet build "Drill4Net.Agent.Worker.csproj" -c Release -o /app/build

FROM build AS publish
ENV MESSAGE_SERVER_ADDRESS $MESSAGE_SERVER_ADDRESS
WORKDIR "/src/src/Agent/Drill4Net.Agent.Service"
RUN dotnet publish "Drill4Net.Agent.Service.csproj" -c Release --runtime linux-x64 -o /app/publish
WORKDIR "/src/src/Agent/Drill4Net.Agent.Worker"
RUN dotnet publish "Drill4Net.Agent.Worker.csproj" -c Release --runtime linux-x64 -o /app/publish
RUN ls -l /app/publish
RUN chmod +x /app/publish/Drill4Net.Agent.Worker.dll

FROM base AS final
# install agent_connector-linuxX64-debug-0.6.0
RUN apt-get update && apt-get install -y wget unzip && \
    wget https://drill4j.jfrog.io/artifactory/drill/com/epam/drill/dotnet/agent_connector-linuxX64-debug/0.6.0/agent_connector-linuxX64-debug-0.6.0.zip && \
    unzip agent_connector-linuxX64-debug-0.6.0.zip && \
    mv linuxX64Debug-0.6.0/libagent_connector.so /usr/lib/
ENV MESSAGE_SERVER_ADDRESS $MESSAGE_SERVER_ADDRESS
ENV DRILL_ADMIN_ADDRESS $DRILL_ADMIN_ADDRESS
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "Drill4Net.Agent.Service.dll"]
