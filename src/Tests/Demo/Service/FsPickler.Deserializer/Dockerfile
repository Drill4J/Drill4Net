
#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Tests/Demo/Service/FsPickler.Deserializer/FsPickler.Deserializer.csproj", "src/Tests/Demo/Service/FsPickler.Deserializer/"]
RUN dotnet restore "src/Tests/Demo/Service/FsPickler.Deserializer/FsPickler.Deserializer.csproj"
COPY . .
WORKDIR "/src/src/Tests/Demo/Service/FsPickler.Deserializer"
RUN dotnet build "FsPickler.Deserializer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FsPickler.Deserializer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ADD ["src/Tests/Demo/Service/FsPickler.Deserializer/test_folder", "test_folder/"]
ENTRYPOINT ["dotnet", "FsPickler.Deserializer.dll"]