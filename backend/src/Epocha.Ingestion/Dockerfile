# Build context is backend/ (see docker-compose.yml), so paths below are relative to that.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY src/Epocha.Domain/Epocha.Domain.csproj src/Epocha.Domain/
COPY src/Epocha.Application/Epocha.Application.csproj src/Epocha.Application/
COPY src/Epocha.Infrastructure/Epocha.Infrastructure.csproj src/Epocha.Infrastructure/
COPY src/Epocha.Ingestion/Epocha.Ingestion.csproj src/Epocha.Ingestion/
RUN dotnet restore src/Epocha.Ingestion/Epocha.Ingestion.csproj

COPY src/Epocha.Domain/ src/Epocha.Domain/
COPY src/Epocha.Application/ src/Epocha.Application/
COPY src/Epocha.Infrastructure/ src/Epocha.Infrastructure/
COPY src/Epocha.Ingestion/ src/Epocha.Ingestion/
RUN dotnet publish src/Epocha.Ingestion/Epocha.Ingestion.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Epocha.Ingestion.dll"]
