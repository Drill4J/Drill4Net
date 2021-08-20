#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Agent/Drill4Net.Agent.Kafka.Service/Drill4Net.Agent.Kafka.Service.csproj", "src/Agent/Drill4Net.Agent.Kafka.Service/"]
RUN dotnet restore "src/Agent/Drill4Net.Agent.Kafka.Service/Drill4Net.Agent.Kafka.Service.csproj"
COPY . .
WORKDIR "/src/src/Agent/Drill4Net.Agent.Kafka.Service"
RUN dotnet build "Drill4Net.Agent.Kafka.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Drill4Net.Agent.Kafka.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Drill4Net.Agent.Kafka.Service.dll"]