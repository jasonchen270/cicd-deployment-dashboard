# CI/CD Deployment Dashboard

A unified dashboard that surfaces build status from GitHub Actions and Jenkins, letting developers retrigger failed jobs without leaving the UI. The API is a .NET (ASP.NET Core) Web API and the frontend is Angular with standalone components and signals.

## Prerequisites

- .NET 10 SDK
- Node 20+ (or 25)
- Docker (optional, for the docker compose run option)

## Usage

### Option 1: native (no Docker)

In one terminal, start the API:

```bash
cd api/CicdDashboard.Api
dotnet run
# API on http://localhost:5085  (Swagger at /swagger)
```

In another, start the Angular dev server:

```bash
cd frontend
npm install
npm start
# UI on http://localhost:4200
```

### Option 2: docker compose

```bash
cd docker
docker compose up --build
# UI on http://localhost:8080 (nginx proxies /api/* to the API container)
```
