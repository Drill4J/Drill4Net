
#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Tests/Serialization/Deserializer.Service.Tests/Deserializer.Service.Tests.csproj", "src/Tests/Serialization/Deserializer.Service.Tests/"]
RUN dotnet restore "src/Tests/Serialization/Deserializer.Service.Tests/Deserializer.Service.Tests.csproj"
COPY . .
WORKDIR "/src/src/Tests/Serialization/Deserializer.Service.Tests"
RUN dotnet build "Deserializer.Service.Tests.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Deserializer.Service.Tests.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ADD ["src/Tests/Serialization/Deserializer.Service.Tests/data", "data/"]
ENTRYPOINT ["dotnet", "Deserializer.Service.Tests.dll"]