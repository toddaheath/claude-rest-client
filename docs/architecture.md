# Restward Architecture

## System Overview

```mermaid
graph LR
    FE[React SPA<br/>Vite + TypeScript<br/>:80] -->|/api/*| API[ASP.NET Core API<br/>.NET 8<br/>:5000]
    API --> DB[(PostgreSQL<br/>:5432)]
    API -->|Proxied HTTP| EXT[External APIs]
```

## Component Architecture

### Frontend (React + TypeScript)

```mermaid
graph TD
    App --> AppLayout
    AppLayout --> Sidebar
    AppLayout --> TopBar
    AppLayout --> RequestPanel
    AppLayout --> ResponsePanel

    Sidebar --> CollectionTree
    Sidebar --> ImportDialog
    Sidebar --> ExportDialog

    TopBar --> EnvironmentSelector
    EnvironmentSelector --> EnvironmentEditor

    RequestPanel --> UrlBar
    RequestPanel --> KeyValueEditor
    RequestPanel --> BodyEditor

    ResponsePanel --> ResponseBody
    ResponsePanel --> ResponseHeaders

    UrlBar --> MethodSelector
```

**State Management**: Zustand store holds active request, response, collections, environments, and UI state.

**API Client**: Fetch wrapper (`api/client.ts`) attaches `X-Api-Key` header to all requests.

### Backend (ASP.NET Core)

```mermaid
graph TD
    MW[ApiKeyAuthMiddleware] --> Controllers
    Controllers --> ProxyController
    Controllers --> CollectionsController
    Controllers --> RequestsController
    Controllers --> FoldersController
    Controllers --> UsersController
    Controllers --> EnvironmentsController
    Controllers --> ImportExportController

    ProxyController --> ProxyService
    ImportExportController --> ImportService
    ImportExportController --> ExportService

    Controllers --> AppDbContext
    AppDbContext --> PostgreSQL[(PostgreSQL)]
```

**Authentication**: API key middleware checks `X-Api-Key` header, looks up user in DB, sets `HttpContext.Items["User"]`. Skips `/health` endpoint.

**Proxy Service**: Executes HTTP requests on behalf of users using `IHttpClientFactory`. Returns status, headers, body, and timing.

## Database Schema

```mermaid
erDiagram
    users ||--o{ collections : owns
    users ||--o{ environments : owns
    collections ||--o{ folders : contains
    collections ||--o{ request_items : contains
    folders ||--o{ folders : nests
    folders ||--o{ request_items : contains
    request_items ||--o{ request_headers : has
    request_items ||--o{ request_parameters : has
    environments ||--o{ environment_variables : has
```

## Request Flow

1. User clicks **Send** in the UI
2. Frontend resolves `{{variables}}` from active environment
3. Frontend POSTs to `/api/proxy` with method, URL, headers, body
4. `ApiKeyAuthMiddleware` validates the API key
5. `ProxyService` executes the HTTP request via `HttpClient`
6. Response (status, headers, body, timing) returned to frontend
7. Frontend displays response with syntax highlighting

## Deployment

```mermaid
graph TD
    subgraph Kubernetes
        ING[Ingress] -->|/api, /health| API_SVC[API Service]
        ING -->|/| FE_SVC[Frontend Service]
        API_SVC --> API_POD[API Pods]
        FE_SVC --> FE_POD[Frontend Pods<br/>nginx]
        API_POD --> PG[PostgreSQL<br/>Bitnami subchart]
    end
```
