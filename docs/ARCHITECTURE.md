# Epocha: architecture

Epocha is an art and history timeline explorer. It ingests artworks from public museum APIs,
stores them in Postgres, and indexes them in Elasticsearch for full-text and faceted search.

## Stack

| Concern | Technology |
| --- | --- |
| API | ASP.NET Core Web API (.NET 10) |
| Source of truth | PostgreSQL 17 via EF Core |
| Search | Elasticsearch 8 |
| Frontend | React + TypeScript (Vite) |
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

## Frontend

`frontend/` is a React + TypeScript app scaffolded with [Vite](https://vite.dev). A few concepts
worth knowing if React is new:

- **Vite** is the build tool and dev server. `npm run dev` serves the app with hot module
  reloading (edits appear in the browser without a full refresh); `npm run build` type-checks
  with `tsc` and produces a static `dist/` bundle for deployment.
- **Components** are functions that return JSX (HTML-like syntax in TypeScript) describing what
  to render, e.g. `App` in `src/App.tsx`. React re-runs a component's function whenever its state
  or props change, and updates only the parts of the real DOM that actually changed.
- **`src/main.tsx`** is the entry point: it mounts `<App />` into the `#root` element declared in
  `index.html`. This is the one place that talks to the DOM directly; everything else is
  components describing what should appear.
- **TypeScript** adds static types on top of JavaScript, checked at build time (`tsc -b`) rather
  than only at runtime. The API's response shapes (`ArtworkSummary`, `ArtworkSearchResult`, ...)
  will be mirrored as TypeScript types, so a mismatch between what the frontend expects and what
  the API actually returns is caught while building rather than as a bug in the browser.

### Talking to the API

The dev server proxies `/api/*` to `http://localhost:5196` (configured in `vite.config.ts`), so
the frontend calls relative URLs like `fetch("/api/artworks/search")` with no CORS setup needed
in development. In production (once containerised) the API will be reachable at a real URL
instead, and the base URL will come from a Vite environment variable rather than being hardcoded.

### State updates and effects

A `useEffect` that fetches data often wants to show a loading state while the request is in
flight. The natural-looking way to do that is to call `setStatus('loading')` as the first line
inside the effect — but React's linter (and the React team) flag that: it's a synchronous state
update from inside an effect, which causes an extra render on every run. The fix depends on where
the change that triggers the effect comes from:

- **`Gallery`** re-fetches when the search text or a filter changes, and every one of those
  changes is caused by a local event: typing in the search box, ticking a filter checkbox. So the
  loading state is set right there in the `onChange`/`onClick` handler instead of in the effect —
  "update it from the event that caused the change," which is what the linter itself suggests.
- **`ArtworkDetailPage`** re-fetches when the `:id` route param changes, and that has no single
  local event to hook into — it could be a click on an `ArtworkCard` in a completely different
  component, the browser's back button, or a URL typed directly. Instead of fighting the linter or
  suppressing the rule, `ArtworkDetailPage` renders `<ArtworkDetailView key={id} .../>`: changing
  `key` makes React discard the old component instance and mount a fresh one whenever `id`
  changes, so `status` starts at `'loading'` through `useState`'s initial value rather than an
  effect resetting it. Same result, no synchronous `setState` in the effect, and it's a standard
  React pattern for "this piece of state should fully reset when this identity changes."

### Running it

```
cd frontend
npm install
npm run dev          # serves on http://localhost:5173; needs the API running for real data
```

## Containers

The whole application runs in Docker: Postgres, Elasticsearch, the API, the frontend, and
ingestion. `docker compose up -d` starts the first four together; ingestion is deliberately left
out of that — it's a batch job that fetches a batch of artworks and exits, not a server, so it
would make no sense to auto-start it (and re-fetch) every time the stack comes up. Run it on
demand instead: `docker compose run --rm ingestion`.

- **`backend/src/Epocha.Api/Dockerfile`** is a multi-stage build: an SDK image compiles and
  publishes the app, then only the published output is copied into a much smaller ASP.NET runtime
  image. The build context is `backend/` (set in `docker-compose.yml`), not the Api project's own
  folder, because the image needs to `COPY` the other projects it references too.
- **Migrations run on startup** (`Program.cs`, via `Database.MigrateAsync()`) rather than needing
  a manual `dotnet ef database update` step. `Migrate()` only applies migrations that haven't run
  yet, so this is a no-op on every restart after the first. This is a reasonable simplification for
  a single-instance app; a system with multiple API replicas would need a separate migration step
  so two containers don't race to alter the schema at once.
- **Configuration comes from environment variables, not `appsettings.Production.json`.** The `api`
  service in `docker-compose.yml` sets `ConnectionStrings__Postgres` and `Elasticsearch__Url`
  directly; the double underscore is ASP.NET Core's convention for nested configuration keys via
  env vars. Docker's internal DNS resolves `postgres` and `elasticsearch` to the right containers,
  the same way `localhost` only worked when running on the host directly.
- **`frontend/Dockerfile`** also multi-stage: a Node image runs `npm run build` to produce a
  static `dist/`, then an `nginx:alpine` image serves just that output — nothing from Node or npm
  ships in the final image.
- **`frontend/nginx.conf`** does two things a static file server doesn't do by default:
  - Proxies `/api/*` to the `api` container, mirroring what `vite.config.ts`'s dev server proxy
    does. The browser only ever talks to one origin, so the API needs no CORS configuration in
    either environment.
  - Falls back to `index.html` for any other path (`try_files $uri /index.html`), which routes
    like `/artworks/45` need: React Router handles them client-side, but without this, a direct
    load or a page refresh on that URL would 404 at the web server before React ever runs.
- **`backend/src/Epocha.Ingestion/Dockerfile`** is the same multi-stage pattern as the API's,
  publishing `Epocha.Ingestion` instead. Its compose service has no `restart` policy and no ports
  — it's meant to be run, finish, and exit, not stay up.
- **A benign warning in both `.NET` containers' logs:** `Cannot load library
  libgssapi_krb5.so.2`. Npgsql probes for optional GSSAPI/Kerberos authentication support, which
  the minimal runtime image doesn't include; it fails to load and falls back to standard
  password authentication, which is what the connection string actually uses. Harmless.

## Design decisions

- **Idempotent ingestion.** Rows are matched on `(SourceSystem, SourceExternalId)`, which has a unique index, so re-running the job never duplicates data.
- **Postgres is the source of truth.** It commits first. If Elasticsearch is unavailable the failure is logged and the affected rows stay pending.
- **Drift repair without a queue.** `Artwork.LastIndexedAt` records the last successful index. Rows where it is null or older than `UpdatedAt` are re-indexed. A transactional outbox is a possible later replacement.
- **Explicit Elasticsearch mapping.** Text fields are analysed for search; keyword fields back filters and facets (era, medium, movement, artist).
- **Derived fields stored once.** `Era` and `MediumCategory` are computed at ingestion so both stores agree.
- **Messy museum data is normalised.** Dates keep the display string plus a numeric range, and free-text mediums are bucketed into categories.
- **Movements come from a curated catalog.** The museum's "style" data mixes movements with centuries, cultures and dynasties, so only values in `MovementCatalog` are kept, and variants ("Analytical Cubism") fold into one canonical movement. Each ingestion run also reconciles movements already stored, so extending the catalog fixes existing data.

## Running locally

**Full stack, one command** — Postgres, Elasticsearch, the API (migrating itself on startup) and
the frontend, all in Docker:

```
copy .env.example .env
docker compose up -d --build
```

The app is then at `http://localhost:3000`. The database starts empty; run ingestion once (see
below) to populate it.

**Local development** — infrastructure in Docker, API/frontend on the host with hot reload:

```
copy .env.example .env
docker compose up -d postgres elasticsearch
cd backend/src/Epocha.Infrastructure
dotnet ef database update --startup-project ../Epocha.Api/Epocha.Api.csproj
cd ../Epocha.Api
dotnet run        # set DOTNET_ENVIRONMENT=Development; serves on http://localhost:5196
cd ../../../frontend
npm install
npm run dev        # serves on http://localhost:5173, proxying /api to the API above
```

**Ingestion** — containerized (against the Docker Postgres/Elasticsearch):

```
docker compose run --rm ingestion
```

or, against whichever Postgres/Elasticsearch you're running locally instead:

```
cd backend/src/Epocha.Ingestion
dotnet run        # set DOTNET_ENVIRONMENT=Development
```

## Status

Done: domain model, persistence and migrations, ingestion into Postgres and Elasticsearch,
search endpoint (filters, facets, sorting, paging), detail endpoint, movement data cleanup,
frontend (gallery, filters, sorting, pagination, detail page). The entire application is
containerized: Postgres, Elasticsearch, API, frontend and ingestion.

The MVP from the original build order is complete. Possible next steps: the Met API as a second
`IArtworkSource`, an artist search filter, the timeline view, deploying it somewhere public.


an ingestion write, and a detail page should always show the current row. It projects straight into the DTO with .Select(...), so EF Core only asks Postgres for the columns actually needed, not the whole entity graph.

Api
- Controllers/ArtworksController.cs now has GET /api/artworks/{id:int}, returning the artwork or a 404. The {id:int} route constraint means a non-integer id doesn't match the route at all, so it also comes back as 404 rather than a validation error — standard ASP.NET Core behavior, not a bug.
