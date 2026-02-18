# Expense Control

A modern web application for tracking expenses, incomes, and budgets, featuring a .NET 10 Web API backend and a React frontend.

## Features
- **Dashboard**: Overview of balances, recent transactions, top categories, and a quick view of monthly budgets with visual gauges.
- **Transactions**: Add, edit, and list expenses/incomes with category and date filtering.
- **Budgets**: Set monthly budgets per expense category, view usage with compact gauge visualizations, and monitor budget health.
- **Categories**: Organize transactions and budgets by customizable categories (Expense/Income).

## Project Structure
```
Backend/
  ExpenseControl.Api/       # .NET 10 Web API (Minimal APIs)
    Features/               # Endpoints for Transactions, Budgets, Balance, etc.
    Entities/               # EF Core entities
    Persistence/            # DbContext and migrations
    Program.cs              # API setup, endpoints
  ExpenseControl.Api.Tests/ # xUnit unit tests

Frontend/
  ExpenseControl.React/     # React frontend (Vite + TypeScript)
    src/                    # Source code
    docker/                 # Docker configuration
```

## Getting Started

### Running with Docker Compose (Recommended)

The easiest way to run the entire application:

```bash
docker compose up --build
```

This starts:
- **PostgreSQL** on `localhost:5432`
- **API** on `http://localhost:5293`
- **Frontend** on `http://localhost:3000`

### Running Locally (Development)

#### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- Docker (for PostgreSQL)

#### Backend
1. Start PostgreSQL:
   ```bash
   docker compose up expensecontrol-postgres
   ```
2. Run the API:
   ```bash
   dotnet run --project Backend/ExpenseControl.Api
   ```
3. API will be available at `http://localhost:5293`

#### Frontend
1. Install dependencies:
   ```bash
   cd Frontend/ExpenseControl.React
   npm install
   ```
2. Start the development server:
   ```bash
   npm run dev
   ```
3. Frontend will be available at `http://localhost:5173`

## Key Technologies
- **Backend**: .NET 10 Minimal APIs, EF Core
- **Frontend**: React, TypeScript, Vite, Bootstrap
- **Testing**: xUnit (backend unit tests), Playwright (frontend end-to-end tests)
- **Database**: PostgreSQL

## Running Tests

### Backend Unit Tests (xUnit)

The .NET backend includes comprehensive unit tests using [xUnit](https://xunit.net/) with AutoFixture, Moq, and EF Core InMemory for testing.

#### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)

#### Running the tests
Run all backend unit tests:
```bash
dotnet test
```

This runs 82 unit tests covering Transactions, Budgets, Balance, Categories, and Households features.

### Frontend End-to-End Tests (Playwright)

The React frontend includes end-to-end tests built with [Playwright](https://playwright.dev/). Tests cover navigation, the Dashboard, Transactions, and Budgets pages.

#### Prerequisites
- [Node.js 20+](https://nodejs.org/)

#### Setup
1. Install dependencies (includes Playwright):
   ```bash
   cd Frontend/ExpenseControl.React
   npm install
   ```
2. Install Playwright browsers:
   ```bash
   npx playwright install chromium
   ```

#### Running the tests
Run all end-to-end tests:
```bash
npm run test:e2e
```

This automatically starts the Vite development server and runs the tests against it. API calls are mocked, so the backend does not need to be running. This runs 40 end-to-end tests covering navigation, Dashboard, Transactions, and Budgets pages.

To see the HTML test report after a run:
```bash
npx playwright show-report
```

## Customization
- Add new categories, budgets, or transaction types via the UI
- Extend endpoints or frontend components as needed

## Contributing
Pull requests and issues are welcome! Please ensure code is clean and tested.

---

**Expense Control** — Modern, visual, and open-source personal finance management.