# Deploying with Coolify

[Coolify](https://coolify.io) is a self-hosted PaaS you run on your own server (here, a Hetzner
VPS) that can deploy a `docker-compose.yml` directly. Because it uses the same compose file as
local dev, most of the setup below is configuration through Coolify's UI rather than new files in
this repo — unlike a platform with its own per-service deployment model, service-name resolution
(`api`, `postgres`, `elasticsearch`) just works exactly as it does locally, since Coolify keeps
every service in the compose file on one Docker network.

I haven't driven an actual Coolify deployment myself, so treat the exact UI steps below as
"roughly this, verify as you go" rather than a guaranteed click-by-click guide — the concepts
(one Docker Compose resource, one public domain on the frontend, secrets set through its env var
UI) are what matter, and we can work out this project's specific instance's exact wording together.

## 1. Point Coolify at this repo

Create a new **Docker Compose** resource in Coolify (not a generic "application" or Dockerfile
resource), pointing it at this repo and branch, with the compose file path set to `docker-compose.yml`
at the repo root.

## 2. Set real secrets

`docker-compose.yml`'s `${VAR:-default}` fallbacks (`epocha_dev_password`, the committed JWT
secret, etc.) exist so `docker compose up` works out of the box for local development — **do not
run this in production with those defaults.** Coolify lets you define environment variables for a
Docker Compose resource, which it exports before running `docker compose up`, the same role a
local `.env` file plays. Set at least:

- `POSTGRES_PASSWORD` — a real random value
- `JWT_SECRET` — a real random value (e.g. `openssl rand -base64 48`); this one matters most,
  since it's what signs login tokens
- `POSTGRES_DB` / `POSTGRES_USER` if you want to change them from the defaults

## 3. Expose only the frontend

Assign a public domain (Coolify's FQDN setting) to the **`frontend`** service only. `api`,
`postgres`, `elasticsearch` and `kibana` should get no public domain — they don't need one.
`nginx.conf.template` already proxies `/api/*` to `http://api:8080` internally (see
`docs/ARCHITECTURE.md`'s Containers section), so the browser only ever talks to the frontend's
domain, exactly like local dev; the API is reachable from the internet only if you deliberately
expose it too, which this setup doesn't need.

Point your domain's DNS A record at your Hetzner server's IP before assigning it in Coolify, so
Coolify's automatic HTTPS (Let's Encrypt) can issue a certificate for it.

## 4. Deploy

Trigger a deploy from Coolify. It runs `docker compose up` (or equivalent) against the file as-is;
the `ingestion` service's `profiles: ["jobs"]` means it's correctly skipped on every deploy, the
same as it is locally — nothing re-fetches artworks just because you redeployed.

Postgres's and Elasticsearch's `127.0.0.1`-only port bindings (see `docker-compose.yml`) mean
they're unreachable from outside the server entirely, which matters more here than it did on your
own machine — this is now a public-facing server, not localhost.

## 5. Populate the database

Since you own this server outright (unlike a managed platform), the simplest way is SSH directly
in and run the job as a one-off, the same command as local dev:

```
ssh <your-server>
cd <path Coolify checked the repo out to>
docker compose --profile jobs run --rm ingestion
```

Re-run any time for more artworks — it's idempotent (see the Design decisions section of
`docs/ARCHITECTURE.md`).

## Notes

- **`/health`** (added for the earlier Fly attempt) is still useful here if Coolify's UI lets you
  point a health check at a specific path — not required, just available.
- **Volumes**: `postgres-data` and `elasticsearch-data` are plain named Docker volumes in the
  compose file; Docker creates them on the server's own disk automatically on first `up`, no
  separate provisioning step needed (unlike Fly's explicit `fly volumes create`).
- If Coolify's actual behavior around any of this (domain assignment, env var injection, one-off
  commands) doesn't match what's described here, tell me exactly what you're seeing and we'll
  adjust — same as working through the Fly attempt.
