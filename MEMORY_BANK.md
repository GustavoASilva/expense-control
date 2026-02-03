# Memory Bank — Expense Control

Purpose
-------
This file captures architecture notes, decisions, rationale, and links to important files so we can track why things were chosen and what open questions remain.

Project Overview
----------------
- Backend: .NET 8 Minimal API (ExpenseControl.Api)
- Database: PostgreSQL (docker-compose uses postgres:16)
- Monitoring: OpenTelemetry + Prometheus + Grafana

Tech Stack (explicit)
---------------------
- .NET Target: net8.0 (API)
- EF Core with Npgsql provider

Key Decisions (inferred / explicit)
----------------------------------
- Use .NET 8 Minimal APIs for a small, focused backend surface.
  - Consequence: Endpoints defined as extension methods under `Features/`.
- Use PostgreSQL as primary DB; migrations are applied at startup in `Program.cs`.
  - Consequence: docker-compose provides a postgres service and the API depends on its health.
- Use EF Core with explicit model configuration in `ExpenseDbContext` (indexes, column types, TPH discriminator for Transaction type).
- Include OpenTelemetry and expose a Prometheus scraping endpoint; Docker-compose wires Prometheus and Grafana.

Rationale & Notes
-----------------
- Migrations auto-run in startup to ease local development and CI.
- CORS is permissive (AllowAnyOrigin/AnyHeader/AnyMethod) for development ease.

Unresolved Questions / To Decide
-------------------------------
- Production settings: connection strings, secrets, and CORS policy need tightening.
- Authentication/authorization is not present — decide on approach (OIDC, JWT, local tokens).
- Backup / migration strategy for production Postgres (volume + backups).

Links (key files)
------------------
- README: README.md
- Solution: expense-control.sln
- Docker compose: docker-compose.yml
- API project file: ExpenseControl.Api/ExpenseControl.Api.csproj
- API Program: ExpenseControl.Api/Program.cs
- DbContext: ExpenseControl.Api/Persistence/ExpenseDbContext.cs

How to use this file
--------------------
- Add new decision entries under "Key Decisions" with date and author.
- When a decision is implemented (code/config changes), add a short link to the PR or commit hash and mark the entry as implemented.

Initial entries created on: 2026-02-02
