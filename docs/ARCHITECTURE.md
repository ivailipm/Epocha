# Epocha: architecture

Epocha is an art and history timeline explorer. It ingests artworks from public museum APIs,
stores them in Postgres, and indexes them in Elasticsearch for full-text and faceted search.

## Stack

| Concern | Technology |
| --- | --- |
| API | ASP.NET Core Web API (.NET 10) |
| Source of truth | PostgreSQL 17 via EF Core |
| Search | Elasticsearch 8 |
| Frontend | React + TypeScript (planned) |
| Local infrastructure | Docker Compose (Postgres and Elasticsearch) |

## Solution layout

```
backend/src
  Epocha.Domain          Entities, enums and pure domain logic. No dependencies.
  Epocha.Application     Use cases and the interfaces they need. Depends on Domain only.
  Epocha.Infrastructure  EF Core, Elasticsearch and museum API clients. Implements Application's interfaces.
  Epocha.Api             HTTP endpoints. Composition root for the web app.
  Epocha.Ingestion       Worker that runs the ingestion job. Composition root for the job.
backend/tests
  Epocha.Application.Tests
```

Dependencies point inward: Api and Ingestion reference Infrastructure and Application,
Infrastructure references Application, and Application references Domain. Application defines
interfaces such as `IArtworkSource`, `IArtworkSearchIndexer` and `IEpochaDbContext`;
Infrastructure implements them, so use-case code never touches HTTP clients, Elasticsearch or
provider-specific EF code.

## Ingestion flow

1. `IArtworkSource` (Art Institute of Chicago client) fetches a page and maps it to museum-neutral `ArtworkRecord`s.
2. `ArtworkIngestionService` upserts artists, movements and artworks into Postgres in one transaction per page.
3. Artworks that are new or changed are projected to `ArtworkSearchDocument` and bulk-indexed into Elasticsearch.
4. A backlog pass indexes anything in Postgres that is missing from or stale in the index.

## Search API

`GET /api/artworks/search` queries Elasticsearch and returns a page of results plus facet counts.

| Parameter | Meaning |
| --- | --- |
| `q` | Full-text query (title, artist, movement, description, medium), typo-tolerant |
| `era`, `medium`, `movement`, `artist` | Filters; repeat a parameter to select several (OR within a facet, AND across facets) |
| `yearFrom`, `yearTo` | Inclusive range on the artwork's start year (negative is BC) |
| `sort` | `Relevance` (default), `DateAsc`, `DateDesc`, `TitleAsc`, `TitleDesc` |
| `page`, `pageSize` | Paging; page size is capped at 100 and results at the first 10,000 |

Facet counts for each facet are computed with that facet's own filter removed, so selecting
"Baroque" still shows how many artworks each other era would return. Invalid input returns a
400 problem-details response.

`GET /api/artworks/{id}` returns the full record for one artwork, read directly from Postgres
rather than Elasticsearch, so it is always current even if indexing is lagging. Returns 404 if
the id doesn't exist.

## Design decisions

- **Idempotent ingestion.** Rows are matched on `(SourceSystem, SourceExternalId)`, which has a unique index, so re-running the job never duplicates data.
- **Postgres is the source of truth.** It commits first. If Elasticsearch is unavailable the failure is logged and the affected rows stay pending.
- **Drift repair without a queue.** `Artwork.LastIndexedAt` records the last successful index. Rows where it is null or older than `UpdatedAt` are re-indexed. A transactional outbox is a possible later replacement.
- **Explicit Elasticsearch mapping.** Text fields are analysed for search; keyword fields back filters and facets (era, medium, movement, artist).
- **Derived fields stored once.** `Era` and `MediumCategory` are computed at ingestion so both stores agree.
- **Messy museum data is normalised.** Dates keep the display string plus a numeric range, and free-text mediums are bucketed into categories.
- **Movements come from a curated catalog.** The museum's "style" data mixes movements with centuries, cultures and dynasties, so only values in `MovementCatalog` are kept, and variants ("Analytical Cubism") fold into one canonical movement. Each ingestion run also reconciles movements already stored, so extending the catalog fixes existing data.

## Running locally

```
copy .env.example .env
docker compose up -d
cd backend/src/Epocha.Infrastructure
dotnet ef database update --startup-project ../Epocha.Api/Epocha.Api.csproj
cd ../Epocha.Ingestion
dotnet run        # set DOTNET_ENVIRONMENT=Development
```

## Status

Done: domain model, persistence and migrations, ingestion into Postgres and Elasticsearch,
search endpoint (filters, facets, sorting, paging), detail endpoint, movement data cleanup.
Next: React frontend, then containerising the API and frontend.


an ingestion write, and a detail page should always show the current row. It projects straight into the DTO with .Select(...), so EF Core only asks Postgres for the columns actually needed, not the whole entity graph.

Api
- Controllers/ArtworksController.cs now has GET /api/artworks/{id:int}, returning the artwork or a 404. The {id:int} route constraint means a non-integer id doesn't match the route at all, so it also comes back as 404 rather than a validation error — standard ASP.NET Core behavior, not a bug.
