# CI/CD Deployment Dashboard

A unified dashboard that surfaces build status from **GitHub Actions** and **Jenkins**, letting developers retrigger failed jobs without leaving the UI.

- **API**: .NET 10 (ASP.NET Core) Web API with xUnit tests
- **Frontend**: Angular 18 (standalone components, signals) with Karma/Jasmine tests
- **Packaging**: Multi-stage Dockerfiles + docker-compose
- **Deployment target**: AWS ECS (Fargate) behind an Application Load Balancer
- **CI pipeline**: GitHub Actions, where tests gate every push before images are built and pushed to ECR

## Repository layout

```
cicd-deployment-dashboard/
├── api/                              # .NET solution
│   ├── CicdDashboard.sln
│   ├── CicdDashboard.Api/            # ASP.NET Core Web API
│   │   ├── Controllers/              # BuildsController, HealthController
│   │   ├── CiClients/                # ICiClient + GitHub/Jenkins implementations
│   │   ├── Models/                   # Build, BuildStatus, BuildSource records
│   │   ├── Services/                 # BuildAggregator
│   │   ├── Program.cs
│   │   └── Dockerfile
│   └── CicdDashboard.Api.Tests/      # xUnit tests (18 tests)
├── frontend/                         # Angular SPA
│   ├── src/app/                      # standalone components + service
│   ├── Dockerfile
│   ├── nginx.conf                    # serves SPA and proxies /api/*
│   └── karma.conf.js
├── docker/
│   └── docker-compose.yml            # local stack: api + frontend
└── deploy/
    ├── .github/workflows/ci.yml      # in a real repo this lives at the repo root
    ├── ecs-task-api.json
    ├── ecs-task-frontend.json
    └── README.md                     # ECS/ALB deployment guide
```

> **Note:** GitHub Actions requires the workflow file at `.github/workflows/` of the repo root. It's parked under `deploy/` here so all deployment artifacts live together; copy it up one level when you initialize the real repo.

## API endpoints

| Method | Path                              | Description                                          |
|--------|-----------------------------------|------------------------------------------------------|
| GET    | `/api/builds`                     | List recent builds across all sources                |
| GET    | `/api/builds?status=Failed`       | Filter by status (Queued/Running/Success/Failed/Canceled) |
| GET    | `/api/builds?source=Jenkins`      | Filter by source (GitHubActions/Jenkins)             |
| POST   | `/api/builds/{id}/retrigger`      | Retrigger a build; id prefix determines the client   |
| GET    | `/api/health`                     | Liveness probe                                       |
| GET    | `/swagger`                        | Swagger UI (Development env only)                    |

The CI clients are seeded with deterministic mock data so the dashboard works out of the box without GitHub or Jenkins credentials. Swapping in real HTTP clients only requires editing `GitHubActionsClient.cs` / `JenkinsClient.cs`.

## Run locally

**Prereqs:** .NET 10 SDK, Node 20+ (or 25), and either Docker or run natively.

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

## Tests

```bash
# Backend (18 xUnit tests)
cd api && dotnet test

# Frontend (11 Karma/Jasmine tests)
cd frontend && npm test
```

Total: **29 tests**, all passing.

## Deployment

See [`deploy/README.md`](deploy/README.md) for the ECS architecture diagram, AWS resource list, and bootstrap commands.
