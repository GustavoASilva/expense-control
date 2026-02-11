# Memory Bank — Expense Control

Purpose
-------
This file captures architecture notes, decisions, rationale, and links to important files so we can track why things were chosen and what open questions remain.

Project Overview
----------------
- Backend: .NET 8 Minimal API (ExpenseControl.Api)
- Frontend: React + TypeScript + Vite (expense-control-react)
- Database: PostgreSQL (docker-compose uses postgres:16)

Tech Stack (explicit)
---------------------
- .NET Target: net8.0 (API)
- EF Core with Npgsql provider
- React 19 with TypeScript
- Vite for frontend build tooling

Key Decisions (inferred / explicit)
----------------------------------
- Use .NET 8 Minimal APIs for a small, focused backend surface.
  - Consequence: Endpoints defined as extension methods under `Features/`.
- Use PostgreSQL as primary DB; migrations are applied at startup in `Program.cs`.
  - Consequence: docker-compose provides a postgres service and the API depends on its health.
- Use EF Core with explicit model configuration in `ExpenseDbContext` (indexes, column types, TPH discriminator for Transaction type).
- Docker Compose orchestrates all services (postgres, api, frontend).

Rationale & Notes
-----------------
- Migrations auto-run in startup to ease local development and CI.
- CORS is permissive (AllowAnyOrigin/AnyHeader/AnyMethod) for development ease.
- Frontend served via nginx in production Docker container.

Unresolved Questions / To Decide
-------------------------------
- Production settings: connection strings, secrets, and CORS policy need tightening.
- Backup / migration strategy for production Postgres (volume + backups).

Resolved Decisions
------------------
- Authentication: AWS Cognito with JWT Bearer tokens.
  - Backend: `Microsoft.AspNetCore.Authentication.JwtBearer` validates Cognito-issued tokens.
  - Frontend: `aws-amplify` v6 handles sign-in/sign-out and attaches access tokens via axios interceptor.
  - All API endpoints require authorization via route group.
  - Configuration: `Cognito:Region`, `Cognito:UserPoolId`, `Cognito:AppClientId` in appsettings; `VITE_COGNITO_USER_POOL_ID`, `VITE_COGNITO_APP_CLIENT_ID` in frontend env.

Links (key files)
------------------
- README: README.md
- Solution: expense-control.sln
- Docker compose: docker-compose.yml
- API project file: ExpenseControl.Api/ExpenseControl.Api.csproj
- API Program: ExpenseControl.Api/Program.cs
- DbContext: ExpenseControl.Api/Persistence/ExpenseDbContext.cs
- Frontend package.json: expense-control-react/package.json

How to use this file
--------------------
- Add new decision entries under "Key Decisions" with date and author.
- When a decision is implemented (code/config changes), add a short link to the PR or commit hash and mark the entry as implemented.

Initial entries created on: 2026-02-02
Updated on: 2026-02-03 (removed Prometheus/Grafana, added frontend to docker-compose)
