# CI/CD Deployment Dashboard

A unified dashboard that surfaces build status from **GitHub Actions** and **Jenkins**, letting developers retrigger failed jobs without leaving the UI.

- **API**: .NET 10 (ASP.NET Core) Web API with xUnit tests
- **Frontend**: Angular 18 (standalone components, signals) with Karma/Jasmine tests
- **Packaging**: Multi-stage Dockerfiles + docker-compose
- **Deployment target**: AWS ECS (Fargate) behind an Application Load Balancer
- **CI pipeline**: GitHub Actions, where tests gate every push before images are built and pushed to ECR

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
