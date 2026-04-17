# zh-learning-core

Chinese learning platform core services.

## Architecture

- `fe-hanzi-anhvu`: Next.js frontend.
- `backend/src/aspire-hanzi-anhvu/aspire-hanzi-anhvu.AppHost`: .NET Aspire AppHost.

Request flow:

`Frontend -> .NET API`

## Prerequisites

- Node.js 20+
- .NET SDK 8.0+
- Docker + Docker Compose
- Docker Desktop running (for Redis/Postgres/Elasticsearch containers)

## Recommended local workflow

From repository root:

### 1) Run backend (.NET local build) with Aspire AppHost

Go to AppHost folder and run:

```bash
cd .\backend\src\aspire-hanzi-anhvu\aspire-hanzi-anhvu.AppHost\
dotnet run
```

When AppHost starts, infrastructure services are started in Docker:

- Redis
- PostgreSQL
- Elasticsearch (with Kibana)

The backend API is still built and run locally by .NET.

### 2) Run frontend (local)

From repository root, go to frontend folder and run:

```bash
cd .\fe-hanzi-anhvu\
npm i
npm run dev:https
```

If you want to create a trusted local HTTPS certificate (recommended for first-time setup), run:

```bash
cd .\fe-hanzi-anhvu\
mkcert -install
mkcert -key-file certificates/localhost-key.pem -cert-file certificates/localhost.pem localhost 127.0.0.1 ::1
```

Note: `npm run dev:https` (`next dev --experimental-https`) can auto-generate a local certificate when needed.
