FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG configuration=Release
WORKDIR /src

# Copy csproj and restore
COPY ["src/StackFood.Lambda/StackFood.Lambda.csproj", "src/StackFood.Lambda/"]
RUN dotnet restore "src/StackFood.Lambda/StackFood.Lambda.csproj" \
    -r linux-musl-x64 \
    /p:PublishReadyToRun=true

# Copy source
COPY . .
WORKDIR "/src/src/StackFood.Lambda"

# Publish
RUN dotnet publish "StackFood.Lambda.csproj" \
    -c $configuration \
    -o /app/publish \
    -r linux-musl-x64 \
    --self-contained false \
    /p:UseAppHost=false \
    /p:PublishReadyToRun=true \
    /p:PublishTrimmed=false \
    --no-restore

FROM base AS final
WORKDIR /app

# Instalar dependências necessárias para .NET no Alpine
RUN apk add --no-cache \
    icu-libs \
    tzdata

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StackFood.Lambda.dll"]