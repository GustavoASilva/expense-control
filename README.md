# Expense Control

A modern web application for tracking expenses, incomes, and budgets, featuring a .NET 8 Web API backend with OpenTelemetry/Prometheus monitoring.

## Features
- **Dashboard**: Overview of balances, recent transactions, top categories, and a quick view of monthly budgets with visual gauges.
- **Transactions**: Add, edit, and list expenses/incomes with category and date filtering.
- **Budgets**: Set monthly budgets per expense category, view usage with compact gauge visualizations, and monitor budget health.
- **Categories**: Organize transactions and budgets by customizable categories (Expense/Income).
- **Metrics**: Prometheus/OpenTelemetry integration for API metrics and health monitoring.

## Project Structure
```
ExpenseControl.Api/         # .NET 8 Web API (Minimal APIs)
  Features/                 # Endpoints for Transactions, Budgets, Balance, etc.
  Entities/                 # EF Core entities
  Persistence/              # DbContext and migrations
  Program.cs                # API setup, metrics, endpoints
```

## Getting Started

### Running Locally

#### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Docker, Docker Compose

#### Backend
- Configure your connection string in `ExpenseControl.Api/appsettings.Development.json`.
- Run database migrations (auto-applied on startup).
- Start the API:
  ```bash
  dotnet run --project ExpenseControl.Api
  ```
- API will be available at `http://localhost:5293` (default).
- Prometheus metrics at `http://localhost:5293/metrics`.

#### Docker Compose
- To run everything (API, DB, Prometheus, Grafana) via Docker:
  ```bash
  docker-compose up --build
  ```

## Key Technologies
- **Backend**: .NET 8 Minimal APIs, EF Core, OpenTelemetry, Prometheus
- **Database**: PostgreSQL (default, configurable)
- **Monitoring**: Prometheus, Grafana (pre-configured dashboards)

## Metrics & Monitoring
- API exposes `/metrics` for Prometheus scraping (transactions, budgets, etc.)
- Health checks at `/health`
- Grafana dashboards available via Docker Compose

## Customization
- Add new categories, budgets, or transaction types via the UI
- Extend endpoints or frontend components as needed

## Contributing
Pull requests and issues are welcome! Please ensure code is clean and tested.

---

**Expense Control** — Modern, visual, and open-source personal finance management.