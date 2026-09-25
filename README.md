# Epocha

Epocha is an art and history timeline explorer. It ingests public-domain artworks from museum
APIs, stores them in PostgreSQL, and serves full-text and faceted search over Elasticsearch.

Built as a portfolio project to practice clean architecture in ASP.NET Core, EF Core migrations,
and dual-writing to a relational store and a search index.

## Features

- Ingests artworks from the [Art Institute of Chicago's public API](https://api.artic.edu/docs/) — idempotent, safe to re-run
- Full-text search with typo tolerance, filterable by era, artist, movement, medium and date range
- Multi-select facet counts (selecting one filter value doesn't hide the others)
- Sorting and pagination
- Artwork detail view

See [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) for the layer structure, the ingestion flow
and the design decisions behind them.

## Stack

ASP.NET Core Web API (.NET 10) &middot; PostgreSQL 17 + EF Core &middot; Elasticsearch 8 &middot;
Docker Compose &middot; React + TypeScript (planned)

## Running locally

Requires the .NET 10 SDK and Docker.

```
copy .env.example .env
docker compose up -d
cd backend/src/Epocha.Infrastructure
dotnet ef database update --startup-project ../Epocha.Api/Epocha.Api.csproj
cd ../Epocha.Ingestion
dotnet run          # set DOTNET_ENVIRONMENT=Development first; pulls a batch of artworks
cd ../Epocha.Api
dotnet run          # set DOTNET_ENVIRONMENT=Development first
```

The API listens on `http://localhost:5196`. Sample requests are in
[`backend/src/Epocha.Api/Epocha.Api.http`](backend/src/Epocha.Api/Epocha.Api.http), or try:

```
GET /api/artworks/search?q=landscape&era=Modern&sort=DateAsc
GET /api/artworks/{id}
```

## Tests

```
dotnet test backend/Epocha.slnx
```

## Status

In progress. Backend (ingestion, search, detail endpoint) is done; the frontend is scaffolded
and the gallery/search view is next, then containerising the API and frontend. See the Status section of
[`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) for the current build order.
