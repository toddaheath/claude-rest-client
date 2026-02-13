# Restward Deployment Guide

## Local Development

### Prerequisites
- .NET 8 SDK
- Node.js 20+
- PostgreSQL 16 (or use Docker)

### Backend

```bash
# Start PostgreSQL (via Docker)
docker run -d --name restward-pg -e POSTGRES_USER=restward -e POSTGRES_PASSWORD=restward -e POSTGRES_DB=restward -p 5432:5432 postgres:16-alpine

# Build and run API
dotnet build ClaudeRestClient.sln
cd src/Restward.Api
dotnet run
# API running at http://localhost:5000
# Admin API key printed in console on first run
```

### Frontend

```bash
cd frontend
npm install
npm run dev
# Frontend running at http://localhost:5173
# Proxies /api/* to http://localhost:5000
```

## Docker Compose

```bash
docker compose -f docker/docker-compose.yml up --build

# Frontend: http://localhost
# API: http://localhost:5000
# Admin API key: dev-admin-key-for-local-development
```

Test the API:
```bash
curl -H "X-Api-Key: dev-admin-key-for-local-development" http://localhost:5000/api/users/me
```

Tear down:
```bash
docker compose -f docker/docker-compose.yml down -v
```

## Kubernetes (Helm)

### Prerequisites
- Kubernetes cluster
- Helm 3
- Container images pushed to a registry

### Build & Push Images

```bash
# Build
docker build -f docker/Dockerfile.api -t your-registry/restward-api:latest .
docker build -f docker/Dockerfile.frontend -t your-registry/restward-frontend:latest .

# Push
docker push your-registry/restward-api:latest
docker push your-registry/restward-frontend:latest
```

### Install

```bash
# Update Helm dependencies (downloads Bitnami PostgreSQL subchart)
cd helm/restward
helm dependency update

# Install
helm install restward helm/restward \
  --set api.image.repository=your-registry/restward-api \
  --set frontend.image.repository=your-registry/restward-frontend \
  --set postgresql.auth.password=your-secure-password \
  --set ingress.host=restward.yourdomain.com
```

The admin API key is auto-generated. Retrieve it:
```bash
kubectl get secret restward-admin -o jsonpath="{.data.admin-api-key}" | base64 -d; echo
```

### Upgrade

```bash
helm upgrade restward helm/restward \
  --set api.image.tag=v1.1.0 \
  --set frontend.image.tag=v1.1.0
```

The admin API key persists across upgrades.

### Uninstall

```bash
helm uninstall restward
```

Note: PostgreSQL PVC is retained by default. Delete manually if needed:
```bash
kubectl delete pvc data-restward-postgresql-0
```
