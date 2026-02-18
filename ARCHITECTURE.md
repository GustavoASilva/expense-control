# Architecture Decisions

This document records the key architectural decisions made in the Expense Control project. Each entry explains what was decided and why.

---

## 1. Monorepo with Two Projects

**Decision:** Keep the backend API and the React frontend in a single repository. Backend projects live under `Backend/` (`Backend/ExpenseControl.Api/` and `Backend/ExpenseControl.Api.Tests/`), and the frontend lives under `Frontend/` (`Frontend/ExpenseControl.React/`).

**Why:** Simplifies coordination between frontend and backend changes. A single PR can update both sides when an API contract changes. Docker Compose orchestrates everything from one place. The `Backend/` and `Frontend/` subfolders keep the repo root clean as the number of projects grows.

---

## 2. .NET 10 Minimal APIs (Backend)

**Decision:** Use ASP.NET Core Minimal APIs on .NET 10 instead of traditional controllers. The solution uses the XML-based `.slnx` format.

**Why:** Less boilerplate, faster to develop, and fits the project's current scope. Each endpoint is a small, self-contained static class with a `Map*Endpoint` extension method. No need for controller base classes, filters, or attribute routing. .NET 10 brings the latest C# 14 features and performance improvements.

---

## 3. Vertical Slice Architecture (Backend)

**Decision:** Organize backend code by feature (`Features/Transactions/`, `Features/Budgets/`, etc.) rather than by technical layer (controllers, services, repositories).

**Why:** Each feature folder contains everything needed for that feature — endpoint definitions, request/response types, and query logic. This makes it easy to find all code related to a feature and add new features without touching unrelated code.

**Structure:**
```
Features/
├── Auth/
├── Balance/
├── Budgets/
│   ├── Create/
│   ├── List/
│   └── Usage/
├── Categories/
├── Households/
└── Transactions/
    ├── Create/
    ├── Delete/
    ├── Get/
    ├── List/
    └── Update/
```

For complex features (Transactions, Budgets), each operation gets its own sub-folder. Simpler features (Categories, Households) keep all endpoints in a single file.

---

## 4. No Service or Repository Layer (Backend)

**Decision:** Endpoints query and persist data directly through EF Core's `DbContext` — no intermediary service or repository classes.

**Why:** The current business logic is straightforward enough that an extra abstraction layer would add complexity without clear benefit. If business rules grow more complex, services can be introduced per feature without affecting other features (thanks to the vertical slice structure).

---

## 5. PostgreSQL with EF Core Code-First

**Decision:** Use PostgreSQL as the database, accessed via Entity Framework Core with code-first migrations.

**Why:** PostgreSQL is a robust, open-source database that runs well in containers. Code-first migrations keep the schema version-controlled alongside the application code. Migrations are auto-applied at startup (`db.Database.Migrate()`), keeping the deployment simple.

---

## 6. TPH Inheritance for Transactions

**Decision:** Use Table-Per-Hierarchy (TPH) inheritance — `Expense` and `Income` are subclasses of `Transaction`, stored in a single `Transactions` table with a `Type` discriminator column.

**Why:** Expenses and incomes share the same fields (amount, date, category, description). TPH avoids the complexity of multiple tables and joins while still allowing type-safe code (`new Expense()` vs `new Income()`). The `Type` discriminator is stored as a string for readability.

---

## 7. Household-Based Multi-Tenancy

**Decision:** All data (transactions, budgets) is scoped by a `HouseholdId`. The household ID is passed as a query parameter or in the request body by the client.

**Why:** Supports shared household expense tracking. The household concept allows multiple users to collaborate on the same financial data.

**Known limitation:** There is currently no server-side verification that the authenticated user belongs to the requested household. This is a trust-based model suitable for the current stage of the project.

---

## 8. AWS Cognito Authentication

**Decision:** Use AWS Cognito for user authentication with JWT Bearer tokens.

**Why:** Offloads user management (sign-up, sign-in, token refresh, password recovery) to a managed service. The backend validates Cognito-issued JWTs. The frontend uses AWS Amplify to handle the authentication flow.

**Development mode:** A mock authentication handler auto-authenticates every request in the Development environment (configurable via `Authentication:Type: "Mock"` in `appsettings.Development.json`). This allows local development without a Cognito setup.

---

## 9. React 19 + TypeScript + Vite (Frontend)

**Decision:** Build the frontend with React 19, TypeScript, and Vite.

**Why:** React is the team's preferred UI library. TypeScript catches bugs at compile time and improves IDE support. Vite provides fast dev server startup and hot module replacement compared to older bundlers.

---

## 10. Bootstrap 5 for Styling

**Decision:** Use Bootstrap 5 with custom CSS on top, imported globally. No CSS modules, CSS-in-JS, or Tailwind.

**Why:** Bootstrap provides a solid set of responsive components and utilities out of the box. Custom CSS in a single `index.css` file extends Bootstrap with project-specific styles (stat cards, sidebar, date picker, etc.) using CSS custom properties for theming.

---

## 11. React Context for Authentication State

**Decision:** Use React's Context API (via `AuthContext` + `useAuth` hook) for authentication state. No external state management library.

**Why:** Authentication state (user, tokens, login/logout functions) is the only truly global state in the app. Each page manages its own data via local `useState` and `useEffect`. This keeps things simple — no Redux, Zustand, or similar libraries are needed at this scale.

---

## 12. Axios for API Communication

**Decision:** Use a single Axios instance with interceptors for all API calls.

**Why:** The request interceptor automatically attaches the Bearer token to every request. The response interceptor redirects to `/login` on 401 responses. All API functions are exported from a single `services/api.ts` file with proper TypeScript types.

---

## 13. React Router v7 with Protected Routes

**Decision:** Use React Router v7 with a `ProtectedRoute` wrapper component that redirects unauthenticated users to `/login`.

**Why:** Clean separation between public routes (`/login`) and authenticated routes (`/`, `/transactions`, `/budgets`). The `Layout` component (sidebar + content area) wraps all authenticated pages via React Router's `<Outlet />`.

---

## 14. Modal Forms via createPortal

**Decision:** Render form modals (`TransactionForm`, `BudgetForm`) using React's `createPortal` to mount them at the document root.

**Why:** Avoids z-index and overflow issues that can occur when modals are nested inside scrollable containers. Each form handles its own validation and category fetching.

---

## 15. Chart.js for Data Visualization

**Decision:** Use Chart.js (via `react-chartjs-2`) for the dashboard's monthly overview bar chart.

**Why:** Lightweight charting library that integrates well with React. Covers the current need (bar charts for monthly income vs expenses) without the overhead of a larger visualization framework.

---

## 16. xUnit + InMemory DB for Backend Tests

**Decision:** Backend tests use xUnit with AutoFixture and Moq. Each test gets a fresh EF Core InMemory database.

**Why:** InMemory databases are fast and isolate tests from each other (each test creates a new database with a unique name). AutoFixture reduces boilerplate for creating test data. Tests call endpoint handler logic directly rather than going through the HTTP pipeline, keeping them fast and focused on business logic.

**Trade-off:** Since tests bypass the HTTP pipeline, middleware behavior (auth, CORS, serialization) is not covered by unit tests.

---

## 17. Playwright for Frontend E2E Tests

**Decision:** Use Playwright for end-to-end testing of the React frontend. Tests run against Chromium only.

**Why:** Playwright provides reliable browser automation with auto-waiting and role-based locators. Tests mock both authentication (via localStorage injection) and API calls (via route interception), so they run without a real backend.

**Structure:**
- `tests/mocks.ts` — shared mock data and setup helpers
- One spec file per feature: `dashboard.spec.ts`, `transactions.spec.ts`, `budgets.spec.ts`, `navigation.spec.ts`

---

## 18. Docker Compose for Local and Deployment

**Decision:** Use Docker Compose to orchestrate three services: PostgreSQL, API, and Frontend.

**Why:** One command (`docker compose up --build`) starts the entire stack. Services use health checks and dependency ordering to start in the correct sequence. The API and frontend both use multi-stage Docker builds to produce small production images.

- **API container:** SDK build stage → ASP.NET runtime image
- **Frontend container:** Node build stage → Nginx serving static files with SPA fallback

---

## 19. Nginx for Frontend Static Serving

**Decision:** Serve the production frontend build with Nginx inside the Docker container.

**Why:** Nginx is fast, lightweight, and handles SPA routing (`try_files $uri $uri/ /index.html`). Static assets get long-lived cache headers (`Cache-Control: public, immutable`, 1 year).

---

## 20. Development Seed Data

**Decision:** Automatically seed the database with sample data (mock household, 3 months of transactions, budgets) when running in the Development environment.

**Why:** Developers get a useful dataset immediately after startup without manual setup. The seeding is idempotent — it checks if the mock household already exists before inserting. Static category seed data is part of the EF Core migration itself (`HasData`).

---

## 21. Wide-Open CORS Policy

**Decision:** Allow any origin, header, and method via CORS.

**Why:** Simplifies development when the frontend and backend run on different ports. This should be tightened before production deployment.

---

## 22. API Endpoints Require Authorization by Default

**Decision:** All API routes are grouped under `RequireAuthorization()`. Only the `/health` endpoint is public.

**Why:** Secure by default. Every new endpoint automatically requires a valid JWT unless explicitly opted out.

---

## 23. Enums Serialized as Strings

**Decision:** Configure `System.Text.Json` globally with `JsonStringEnumConverter` so enum values (like `TransactionType.Expense`) are serialized as `"Expense"` instead of `0`.

**Why:** Makes API responses and database values human-readable. The TPH discriminator column also stores the type as a string for the same reason.

---

## 24. Budget Upsert Pattern

**Decision:** The "create budget" endpoint performs an upsert — it creates a new budget or updates an existing one based on the unique combination of `(categoryId, month, year, householdId)`.

**Why:** Only one budget per category per month per household makes sense. The upsert simplifies the client code — it doesn't need to distinguish between creating and editing.

---

## 25. Configuration in a Dedicated Folder

**Decision:** Store `appsettings.json` and environment-specific config files in a `Configuration/` subfolder instead of the project root.

**Why:** Keeps the project root cleaner. The configuration base path is explicitly set in `Program.cs` to load from this folder.
