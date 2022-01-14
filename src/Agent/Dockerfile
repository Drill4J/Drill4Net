#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
ENV KAFKA_SERVER_PORT $KAFKA_SERVER_PORT
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ENV KAFKA_SERVER_PORT $KAFKA_SERVER_PORT
WORKDIR /src
COPY . .
RUN ls -la build || true
RUN rm -rf build/bin 
RUN rm -rf build/obj
RUN ls -la build || true
RUN dotnet restore "src/Agent/Drill4Net.Agent.Service/Drill4Net.Agent.Service.csproj"
RUN dotnet restore "src/Agent/Drill4Net.Agent.Worker/Drill4Net.Agent.Worker.csproj"
WORKDIR "/src/src/Agent/Drill4Net.Agent.Service"
RUN dotnet build "Drill4Net.Agent.Service.csproj" -c Release -o /app/build
WORKDIR "/src/src/Agent/Drill4Net.Agent.Worker"
RUN dotnet build "Drill4Net.Agent.Worker.csproj" -c Release -o /app/build

FROM build AS publish
ENV KAFKA_SERVER_PORT $KAFKA_SERVER_PORT
WORKDIR "/src/src/Agent/Drill4Net.Agent.Service"
RUN dotnet publish "Drill4Net.Agent.Service.csproj" -c Release -o /app/publish
WORKDIR "/src/src/Agent/Drill4Net.Agent.Worker"
RUN dotnet publish "Drill4Net.Agent.Worker.csproj" -c Release -o /app/publish

FROM base AS final
ENV KAFKA_SERVER_PORT $KAFKA_SERVER_PORT
RUN apt-get update && apt-get install -y --no-install-recommends gettext-base && apt-get clean
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /src/src/Agent/Drill4Net.Agent.Service/svc.yml.tamplate .
COPY --from=build /src/src/Agent/Drill4Net.Agent.Service/entrypoint.sh .
RUN ls -la
#ENTRYPOINT [ "dotnet", "Drill4Net.Agent.Service.dll"]
ENTRYPOINT [ "bash", "-c", "./entrypoint.sh && dotnet Drill4Net.Agent.Service.dll"]
