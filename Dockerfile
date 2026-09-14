# =========================
# Build Stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY RM-Backend-API.sln ./
COPY RM-Backend-API/RM-Backend-API.csproj RM-Backend-API/
COPY RM.DataModel/RM.DataModel.csproj RM.DataModel/
COPY RM.DataRepository/RM.DataRepository.csproj RM.DataRepository/
COPY RM.Infrastructure/RM.Infrastructure.csproj RM.Infrastructure/

RUN dotnet restore "RM-Backend-API/RM-Backend-API.csproj"

COPY . .

RUN dotnet publish "RM-Backend-API/RM-Backend-API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false


# =========================
# Runtime Stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 8080

COPY docker-entrypoint.sh ./docker-entrypoint.sh
RUN chmod +x ./docker-entrypoint.sh && sed -i 's/\r$//' ./docker-entrypoint.sh

COPY --from=build /app/publish .

ENTRYPOINT ["/bin/sh", "./docker-entrypoint.sh"]
