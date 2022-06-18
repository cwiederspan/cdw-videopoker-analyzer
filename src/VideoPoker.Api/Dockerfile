FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["VideoPoker.Api/VideoPoker.Api.csproj", "VideoPoker.Api/"]
RUN dotnet restore "VideoPoker.Api/VideoPoker.Api.csproj"
COPY . .
WORKDIR "/src/VideoPoker.Api"
RUN dotnet build "VideoPoker.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VideoPoker.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VideoPoker.Api.dll"]