# Build context is backend/ (see docker-compose.yml), so paths below are relative to that.

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY src/Epocha.Domain/Epocha.Domain.csproj src/Epocha.Domain/
COPY src/Epocha.Application/Epocha.Application.csproj src/Epocha.Application/
COPY src/Epocha.Infrastructure/Epocha.Infrastructure.csproj src/Epocha.Infrastructure/
COPY src/Epocha.Api/Epocha.Api.csproj src/Epocha.Api/
RUN dotnet restore src/Epocha.Api/Epocha.Api.csproj

COPY src/Epocha.Domain/ src/Epocha.Domain/
COPY src/Epocha.Application/ src/Epocha.Application/
COPY src/Epocha.Infrastructure/ src/Epocha.Infrastructure/
COPY src/Epocha.Api/ src/Epocha.Api/
RUN dotnet publish src/Epocha.Api/Epocha.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Epocha.Api.dll"]
