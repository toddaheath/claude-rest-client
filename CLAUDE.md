# Restward — REST Client Web Application

## Build & Test
```bash
# Backend
dotnet build ClaudeRestClient.sln
dotnet test ClaudeRestClient.sln

# Frontend
cd frontend && npm install && npm run build
```

## Run Locally
```bash
# Start PostgreSQL
docker run -d --name restward-pg -e POSTGRES_USER=restward -e POSTGRES_PASSWORD=restward -e POSTGRES_DB=restward -p 5432:5432 postgres:16-alpine

# API (from repo root)
cd src/Restward.Api && dotnet run

# Frontend (from repo root)
cd frontend && npm run dev
```

## Docker Compose
```bash
docker compose -f docker/docker-compose.yml up --build
```

## Project Structure
- `src/ClaudeRestClient/` — Original Claude API client library
- `src/Restward.Api/` — ASP.NET Core Web API (backend)
- `frontend/` — React + TypeScript SPA (Vite)
- `tests/` — xUnit test projects
- `docker/` — Dockerfiles and docker-compose
- `helm/restward/` — Helm chart for Kubernetes deployment
- `docs/` — Architecture and deployment documentation

## Environment URLs

| Environment | Frontend URL | API URL |
|-------------|-------------|---------|
| Local Dev | http://localhost:5173 | http://localhost:5000 |
| Docker Compose | http://localhost | http://localhost:5000 |
| Dev (Azure) | https://restward.dev.heathrobotics.io | https://restward-api.dev.heathrobotics.io |
| Prod (Azure) | https://restward.heathrobotics.io | https://restward-api.heathrobotics.io |

## Conventions
- .NET 8, C# with nullable enabled
- JSON serialization uses `System.Text.Json` with `JsonPropertyName` attributes for snake_case mapping
- xUnit for testing
- API authentication via `X-Api-Key` header
- Frontend state management with Zustand
- All API endpoints under `/api/` prefix
