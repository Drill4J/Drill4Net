
#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Tests/Serialization/Serialization.Pong.Test/Serialization.Pong.Test.csproj", "src/Tests/Serialization/Serialization.Pong.Test/"]
RUN dotnet restore "src/Tests/Serialization/Serialization.Pong.Test/Serialization.Pong.Test.csproj"
COPY . .
WORKDIR "/src/src/Tests/Serialization/Serialization.Pong.Test"
RUN dotnet build "Serialization.Pong.Test.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Serialization.Pong.Test.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ADD ["src/Tests/Serialization/Serialization.Pong.Test/data", "data/"]
ENTRYPOINT ["dotnet", "Serialization.Pong.Test.dll"]