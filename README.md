# Ticket Management System

A web app for tracking requests from submission to resolution, with role-based access so people only see and change what they should.

## The problem

Requests that arrive by email, chat, and spreadsheets are easy to lose. Nobody can say who owns a request, what state it is in, or how urgent it is. Reporting turns into digging through inboxes.

This app puts every request in one place. Each ticket has a title, description, status (Open, In Progress, Closed), and priority (Low, Medium, High). Staff see the tickets they are authorized to access. Administrators can also edit and delete them.

**Who it's for:** small teams that handle a steady flow of requests (support, internal service, records or information requests) and need a simple shared queue. It is also a portfolio project showing a complete .NET + React stack, tested and containerized.

![Ticket list, logged in as an admin](docs/screenshot.png)

![alt text](image-1.png)

## Quick start

You need Git and Docker Desktop. .NET and Node.js are not required.

```bash
git clone <repository-url>
cd TicketManagement
cp .env.example .env     # then set POSTGRES_PASSWORD and JWT_KEY in .env
docker compose up -d
```

Open the app:

| Service | URL |
| --- | --- |
| Frontend | http://localhost:5173 |
| API | http://localhost:5186 |
| API docs (Scalar) | http://localhost:5186/scalar |
| Health check | http://localhost:5186/health |
| PostgreSQL | localhost:5432 |

Never commit `.env`; it holds local secrets. `.env.example` contains placeholders only.

Stop everything with `docker compose down`.

## Features

* JWT login; protected ticket endpoints return `401 Unauthorized` without a token
* Role-based authorization: Admin role required to edit or delete tickets
* Create, view, update, and delete tickets
* Validation on both client and API (invalid input returns `400 Bad Request`)
* Logging via ASP.NET Core
* Backend (xUnit) and frontend (Vitest) tests, run in CI
* Docker Compose, plus a Helm chart for local Kubernetes (kind)

## Tech stack

ASP.NET Core / .NET 10, Entity Framework Core, PostgreSQL 16, React, TypeScript, Vite, xUnit, Vitest, Docker Compose, Kubernetes (kind + Helm), GitHub Actions.

## Project structure

```text
TicketManagement/
├── TicketManagement.Api/        # ASP.NET Core Web API
├── TicketManagement.Api.Tests/  # xUnit tests
├── TicketManagement.Ui/         # React + TypeScript frontend
├── helm/ticket-management/      # Helm chart
├── docker-compose.yml           # Runs the complete application
├── kind-config.yaml             # Local Kubernetes cluster config
├── .env.example                 # Environment variable template
└── README.md
```

## Configuration

| Variable | Purpose |
| --- | --- |
| `POSTGRES_PASSWORD` | PostgreSQL password |
| `JWT_KEY` | Secret key used to sign JWT tokens |

## Authentication

Users log in to receive a JWT, which the frontend sends as `Authorization: Bearer <token>`. The key comes from the `JWT_KEY` environment variable.

Check that protection works:

```bash
curl -i http://localhost:5186/api/Tickets   # 401 Unauthorized
```

## API

* Get tickets / get a ticket by ID
* Create, update, and delete a ticket (update and delete need the Admin role)
* User login

### Ticket status

The C# enum keeps `InProgress` as its identifier but serializes it as `"In Progress"`:

```csharp
public enum TicketStatus
{
    Open,

    [JsonStringEnumMemberName("In Progress")]
    InProgress,

    Closed
}
```

### Validation rules

* Title: 3–100 characters
* Description: 5–500 characters
* Status: Open, In Progress, Closed
* Priority: Low, Medium, High

## Database

The API uses Entity Framework Core with PostgreSQL. In Docker the connection uses `Host=postgres`, `Port=5432`, `Database=TicketManagementDb`. Apply migrations during local development with:

```bash
dotnet ef database update
```

## Testing

```bash
dotnet test                # backend, from the project root
npm test                   # frontend, from TicketManagement.Ui
```

## Kubernetes (kind + Helm)

For local Kubernetes deployment, this project includes a Helm chart (`helm/ticket-management/`) and a [kind](https://kind.sigs.k8s.io/) cluster config (`kind-config.yaml`). Requires `kind`, `kubectl`, and `helm` installed.

### Create the cluster

```bash
kind create cluster --config kind-config.yaml
```

### Build and load images

kind doesn't pull from Docker Hub for local images — build them, then load into the cluster:

```bash
docker build -t ticketmanagement-api-image ./TicketManagement.Api
docker build -t ticketmanagement-ui-image ./TicketManagement.Ui
kind load docker-image ticketmanagement-api-image --name ticket-management-system
kind load docker-image ticketmanagement-ui-image --name ticket-management-system
```

### Install the chart

```bash
helm install ticket-management-system ./helm/ticket-management \
  --set secrets.postgresPassword=yourpassword \
  --set secrets.jwtKey=your-jwt-signing-key-min-32-chars
```

### Access the app

Run each in its own terminal (the UI currently calls the API at `localhost:5186`, so both are needed):

```bash
kubectl port-forward svc/ticket-management-system-ui 5173:80
kubectl port-forward svc/ticket-management-system-api 5186:8080
```

Then open `http://localhost:5173`.

### Tear down

```bash
helm uninstall ticket-management-system
kind delete cluster --name ticket-management-system
```

Note: Postgres has no persistent volume in this chart — data resets whenever the pod restarts. Fine for local/demo use; a real deployment would add a `PersistentVolumeClaim` and (optionally) an Ingress controller instead of `port-forward`.

## CI/CD (GitHub Actions)

### Continuous Integration

`.github/workflows/ci-cd.yml` runs on every push to `main` and on pull requests targeting `main`:

* Restores, builds, and tests the API (`dotnet restore` / `build` / `test`)
* Installs dependencies, builds, and tests the UI (`npm ci`, `npm run build`, `npm test`)

### Continuous Deployment

The `deploy-to-kind` job in the same workflow:

* Runs only when triggered manually (`workflow_dispatch` — a "Run workflow" button on the workflow's page in the **Actions** tab), and only after `build-and-test` passes (`needs:`)
* Builds the API/UI images, spins up an ephemeral `kind` cluster on the runner (`kind-config.yaml`), loads the images, then `helm upgrade --install`s the chart
* Verifies the deployment with `kubectl rollout status` on all three Deployments
* Secrets (`POSTGRES_PASSWORD`, `JWT_KEY`) come from GitHub Actions repository secrets and are passed via `--set` — the runner never sees `.env`, since that file is local-only and gitignored

Before running it, add `POSTGRES_PASSWORD` and `JWT_KEY` as repository secrets: repo **Settings → Secrets and variables → Actions → New repository secret**.
