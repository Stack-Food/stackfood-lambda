FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["src/StackFood.Lambda/StackFood.Lambda.csproj", "src/StackFood.Lambda/"]
RUN dotnet restore "src/StackFood.Lambda/StackFood.Lambda.csproj" /p:PublishReadyToRun=true
COPY . .
WORKDIR "/src/src/StackFood.Lambda"
RUN dotnet build "StackFood.Lambda.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
WORKDIR /src/src/StackFood.Lambda
RUN dotnet publish "StackFood.Lambda.csproj" -c $configuration -o /app/publish /p:UseAppHost=false /p:PublishReadyToRun=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StackFood.Lambda.dll"]
