#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .
RUN ls -la build
RUN rm -rf build/bin
RUN rm -rf build/obj
RUN ls -la build
RUN dotnet restore "src/Agent/Drill4Net.Agent.Worker/Drill4Net.Agent.Worker.csproj"
WORKDIR "/src/src/Agent/Drill4Net.Agent.Worker"
RUN dotnet build "Drill4Net.Agent.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Drill4Net.Agent.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Drill4Net.Agent.Worker.dll"]
