# Expense Control

A modern web application for tracking expenses, incomes, and budgets, featuring a .NET 8 Web API backend and a React frontend.

## Features
- **Dashboard**: Overview of balances, recent transactions, top categories, and a quick view of monthly budgets with visual gauges.
- **Transactions**: Add, edit, and list expenses/incomes with category and date filtering.
- **Budgets**: Set monthly budgets per expense category, view usage with compact gauge visualizations, and monitor budget health.
- **Categories**: Organize transactions and budgets by customizable categories (Expense/Income).

## Project Structure
```
ExpenseControl.Api/         # .NET 8 Web API (Minimal APIs)
  Features/                 # Endpoints for Transactions, Budgets, Balance, etc.
  Entities/                 # EF Core entities
  Persistence/              # DbContext and migrations
  Program.cs                # API setup, endpoints

expense-control-react/      # React frontend (Vite + TypeScript)
  src/                      # Source code
  docker/                   # Docker configuration
```

## Getting Started

### Running with Docker Compose (Recommended)

The easiest way to run the entire application:

```bash
docker-compose up --build
```

This starts:
- **PostgreSQL** on `localhost:5432`
- **API** on `http://localhost:5293`
- **Frontend** on `http://localhost:3000`

### Running Locally (Development)

#### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- Docker (for PostgreSQL)

#### Backend
1. Start PostgreSQL:
   ```bash
   docker-compose up expensecontrol-postgres
   ```
2. Run the API:
   ```bash
   dotnet run --project ExpenseControl.Api
   ```
3. API will be available at `http://localhost:5293`

#### Frontend
1. Install dependencies:
   ```bash
   cd expense-control-react
   npm install
   ```
2. Start the development server:
   ```bash
   npm run dev
   ```
3. Frontend will be available at `http://localhost:5173`

## Key Technologies
- **Backend**: .NET 8 Minimal APIs, EF Core
- **Frontend**: React, TypeScript, Vite, Bootstrap
- **Database**: PostgreSQL

## Customization
- Add new categories, budgets, or transaction types via the UI
- Extend endpoints or frontend components as needed

## Contributing
Pull requests and issues are welcome! Please ensure code is clean and tested.

---

**Expense Control** — Modern, visual, and open-source personal finance management.