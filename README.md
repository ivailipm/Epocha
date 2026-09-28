# Epocha

Epocha is an art and history timeline explorer. It ingests public-domain artworks from museum
APIs, stores them in PostgreSQL, and serves full-text and faceted search over Elasticsearch
through a React frontend.

Built as a portfolio project to practice clean architecture in ASP.NET Core, EF Core migrations,
dual-writing to a relational store and a search index, and containerising the whole stack.

## Features

- Ingests artworks from the [Art Institute of Chicago's public API](https://api.artic.edu/docs/) — idempotent, safe to re-run
- Full-text search with typo tolerance, filterable by era, movement and medium
- Multi-select facet counts (selecting one filter value doesn't hide the others)
- Sorting and pagination
- Artwork detail view
- Accounts with named collections — save artworks, browse and search stay open to everyone
- The entire application (Postgres, Elasticsearch, API, frontend, ingestion) runs in Docker

See [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) for the layer structure, the ingestion flow
and the design decisions behind them.

## Stack

ASP.NET Core Web API (.NET 10) &middot; PostgreSQL 17 + EF Core &middot; Elasticsearch 8 &middot;
React + TypeScript (Vite) &middot; Docker Compose

## Running locally

Requires Docker. (The .NET 10 SDK and Node are only needed for local development without
containers — see [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md#running-locally).)

```
copy .env.example .env
docker compose up -d --build
```

The app is at `http://localhost:3000`. The database starts empty — populate it once with:

```
docker compose run --rm ingestion
```

Sample API requests are in
[`backend/src/Epocha.Api/Epocha.Api.http`](backend/src/Epocha.Api/Epocha.Api.http), or try:

```
GET /api/artworks/search?q=landscape&era=Modern&sort=DateAsc
GET /api/artworks/{id}
```

## Tests

```
dotnet test backend/Epocha.slnx
```

## Deploying

See [`docs/DEPLOYMENT.md`](docs/DEPLOYMENT.md) for deploying the whole stack with
[Coolify](https://coolify.io), self-hosted on your own server.

## Status

The MVP is complete, plus accounts and collections: ingestion, search, detail endpoint, the
frontend (gallery, filters, sorting, pagination, detail page), and user accounts with named
collections, all fully containerised. See the Status section of
[`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) for what's next.
