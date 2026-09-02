# Ticket Management System

A full-stack ticket management application built with ASP.NET Core, React, TypeScript, Entity Framework Core, PostgreSQL, JWT authentication, and Docker.

## Project Overview

The Ticket Management System is a full-stack web application for creating and managing support tickets.

Users can log in and view tickets they are authorized to access. They can create tickets with a title, description, status, and priority. Administrators have additional permissions to edit tickets.

The project demonstrates:

* ASP.NET Core Web API
* Entity Framework Core
* PostgreSQL
* React and TypeScript
* JWT authentication
* Role-based authorization
* Input validation and error handling
* Logging
* Backend and frontend testing
* Docker and Docker Compose
* REST API and frontend integration

The project was built as a practical portfolio application to demonstrate modern full-stack development and the ability to design, build, test, and containerize an application.

## Technologies

* ASP.NET Core / .NET 10 Web API
* Entity Framework Core
* PostgreSQL 16
* React
* TypeScript
* Vite
* JWT Authentication
* Docker / Docker Compose
* xUnit
* Vitest
* Git / GitHub

## Project Structure

```text
TicketManagement/

├── TicketManagement.Api/     # ASP.NET Core Web API
├── TicketManagement.Ui/      # React + TypeScript frontend
├── docker-compose.yml        # Runs the complete application
├── .env.example              # Environment variable template
├── .gitignore
└── README.md
```

## Prerequisites

Install:

* Git
* Docker Desktop

The application can be run using Docker Compose, so .NET and Node.js are not required to run the application through Docker.

## Initial Setup

Clone the repository:

```bash
git clone <repository-url>
cd TicketManagement
```

Create the local environment file:

```bash
cp .env.example .env
```

Open `.env` and provide your own local values:

```env
POSTGRES_PASSWORD=your-postgres-password
JWT_KEY=your-jwt-secret-key
```

Do not commit `.env` to Git. It contains local secrets.

`.env.example` is safe to commit because it contains placeholder values.

## Run the Application

From the project root:

```bash
docker compose up
```

Or run in the background:

```bash
docker compose up -d
```

Docker Compose starts:

* PostgreSQL
* ASP.NET Core API
* React frontend

The application uses a Docker network so the API connects to PostgreSQL using the service name:

```text
Host=postgres
```

Inside Docker, `localhost` refers to the current container. Therefore, the API does not use `localhost` to connect to PostgreSQL.

## Application URLs

Frontend:

```text
http://localhost:5173
```

API:

```text
http://localhost:5186
```

PostgreSQL:

```text
localhost:5432
```

## Authentication

The application uses JWT bearer authentication.

Users must log in before accessing protected ticket endpoints.

The JWT key is provided to the API through the `JWT_KEY` environment variable.

The frontend sends the token using:

```text
Authorization: Bearer <token>
```

The API uses authentication and authorization to protect ticket endpoints.

Administrators have additional permissions to edit tickets.

## Docker

### Build API Image

From the `TicketManagement.Api` folder:

```bash
docker build -t ticketmanagement-api-image .
```

### Build UI Image

From the `TicketManagement.Ui` folder:

```bash
docker build -t ticketmanagement-ui-image .
```

### Validate Compose Configuration

From the project root:

```bash
docker compose config
```

This verifies the resolved Docker Compose configuration, including environment variables and services.

### Start the Application

```bash
docker compose up
```

### Start in the Background

```bash
docker compose up -d
```

### Recreate the API Container

When the API Docker image has been rebuilt after code changes:

```bash
docker compose up -d --force-recreate api
```

`--force-recreate` forces Docker Compose to create a new API container even if the existing container appears unchanged.

It does not rebuild the image.

Typical workflow after API code changes:

```text
Change code
    ↓
docker build
    ↓
docker compose up -d --force-recreate api
```

### View Compose Containers

```bash
docker compose ps
```

This shows containers belonging to the current Docker Compose project.

### View All Running Containers

```bash
docker ps
```

This shows all running Docker containers, including containers outside this project.

### View API Logs

```bash
docker compose logs api
```

View the most recent API logs:

```bash
docker compose logs api --tail 50
```

Follow API logs in real time:

```bash
docker compose logs -f api
```

### View PostgreSQL Logs

```bash
docker compose logs postgres
```

### Stop the Application

```bash
docker compose down
```

`docker compose down` stops and removes the containers and Compose network. The Docker images are not removed.

## Docker Compose Networking

Docker Compose creates a network for the application.

The services are:

```text
postgres
api
ui
```

The API connects to PostgreSQL using:

```text
Host=postgres
Port=5432
```

`postgres` is the PostgreSQL Compose service name and can be resolved by Docker's internal DNS.

The browser connects to the exposed host ports:

```text
Frontend → localhost:5173
API      → localhost:5186
Postgres → localhost:5432
```

The API container connects internally to:

```text
postgres:5432
```

## Database

PostgreSQL runs as a Docker container.

The API uses Entity Framework Core with the PostgreSQL provider.

The database connection used inside Docker is based on:

```text
Host=postgres
Port=5432
Database=TicketManagementDb
```

The `postgres` hostname refers to the PostgreSQL service defined in `docker-compose.yml`.

Database migrations are managed using Entity Framework Core.

Apply migrations during local development with:

```bash
dotnet ef database update
```

## API

The API provides ticket management functionality including:

* Get tickets
* Get a ticket by ID
* Create a ticket
* Update a ticket
* Delete a ticket
* User login

Protected ticket endpoints require a valid JWT.

Editing and deleting tickets require the Admin role.

## Ticket Status

The backend uses the C# enum:

```csharp
public enum TicketStatus
{
    Open,

    [JsonStringEnumMemberName("In Progress")]
    InProgress,

    Closed
}
```

This keeps the C# identifier `InProgress` while exposing the more readable JSON value:

```json
"In Progress"
```

The API therefore accepts:

```json
{
  "status": "In Progress"
}
```

## Development

The backend uses:

* Controllers
* DTOs
* Service layer
* Dependency Injection
* Entity Framework Core
* PostgreSQL
* JWT authentication
* Authorization
* Validation
* Logging
* Automated tests

The frontend uses:

* React
* TypeScript
* Functional components
* React hooks
* API integration
* JWT authentication
* Client-side validation

## Testing

### Backend Tests

The backend uses xUnit.

Run the backend tests from the project root:

```bash
dotnet test
```

### Frontend Tests

The frontend uses Vitest.

From the `TicketManagement.Ui` folder:

```bash
npm test
```

Current frontend tests include `TicketList` rendering tests.

## Validation and Error Handling

The frontend performs basic validation before creating or editing a ticket.

Ticket validation includes:

* Title: 3–100 characters
* Description: 5–500 characters
* Status: Open, In Progress, Closed
* Priority: Low, Medium, High

The API also performs request validation and returns appropriate HTTP responses for invalid requests.

For example, invalid enum values result in a `400 Bad Request`.

Authentication failures result in:

```text
401 Unauthorized
```

## Logging

The API uses ASP.NET Core logging.

Examples include:

* Information logging when tickets are created
* Warning logging when a requested ticket is not found
* Error logging provided by the framework for database/API failures

Logs can be viewed with:

```bash
docker compose logs api
```

## API Authentication Test

An unauthenticated request to the protected ticket endpoint:

```bash
curl -i http://localhost:5186/api/Tickets
```

should return:

```text
401 Unauthorized
```

After logging in and obtaining a JWT, the token can be supplied using:

```text
Authorization: Bearer <token>
```

An authenticated request should return the ticket data.

## Environment Variables

| Variable            | Purpose                            |
| ------------------- | ---------------------------------- |
| `POSTGRES_PASSWORD` | PostgreSQL password                |
| `JWT_KEY`           | Secret key used to sign JWT tokens |

Never commit real secrets to the repository.

`.env` should remain local and should be excluded through `.gitignore`.

## Useful Commands

### Check Docker

```bash
docker --version
docker compose version
```

### Check Docker Context

```bash
docker context ls
```

Docker Desktop normally uses the `desktop-linux` context on macOS.

### Check Git

```bash
git --version
```

### Check Repository Status

```bash
git status
```

### Review Changes

```bash
git diff
```

## Docker Troubleshooting

### Container Name Conflict

If Compose reports that a container name is already in use, check the containers:

```bash
docker compose ps
```

If an old container needs to be removed:

```bash
docker rm -f ticketmanagement-api-container
```

Then recreate it with Compose:

```bash
docker compose up -d
```

The same principle applies to the PostgreSQL and UI containers if their names conflict.

### Dockerfile Not Found

The API Dockerfile is located in:

```text
TicketManagement.Api/Dockerfile
```

Therefore the API image build command should be run from:

```text
TicketManagement.Api
```

```bash
docker build -t ticketmanagement-api-image .
```

The `.` means the current directory is used as the Docker build context.

### Check Container Environment

To inspect environment variables inside the API container:

```bash
docker exec ticketmanagement-api-container printenv
```

To inspect connection-related variables:

```bash
docker exec ticketmanagement-api-container printenv | grep ConnectionStrings
```

## Project Status

The project demonstrates a modern full-stack .NET application with:

* ASP.NET Core Web API
* Entity Framework Core
* PostgreSQL
* React
* TypeScript
* JWT authentication
* Authorization
* Validation
* Logging
* Backend unit testing
* Frontend unit testing
* Docker
* Docker Compose
* Environment-based configuration
* Git/GitHub

The application can be run as a complete containerized stack using Docker Compose.
