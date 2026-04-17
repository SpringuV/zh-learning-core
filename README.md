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
- mkcert (optional, for trusted local HTTPS certificate)

## Run with Docker

From repository root:

Backend only (recommended first step):

```bash
docker compose up --build
```

Include frontend too:

```bash
docker compose --profile frontend up --build
```

Services:

- Frontend: `http://localhost:3000`
- .NET API: `http://localhost:8080`

## Local run without Docker

### 1) Run backend (.NET)

From repository root, go to AppHost folder and run:

```bash
cd .\backend\src\aspire-hanzi-anhvu\aspire-hanzi-anhvu.AppHost\
dotnet run
```

### 2) Run frontend

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
