# Expense Control Web Application

A modern web application for managing personal or business expenses with a clean architecture and intuitive user interface.

## 🏗️ Architecture Overview

This application follows a **microservices-inspired** architecture with clear separation between backend and frontend:

- **Backend**: .NET 8 Web API with Vertical Slice Architecture
- **Frontend Options**:
  - **React** (NEW): Modern React 19 with TypeScript, Vite, and Chart.js
  - **Blazor WebAssembly**: Original Blazor WASM implementation
- **Database**: PostgreSQL (Docker)
- **Observability**: OpenTelemetry with Prometheus metrics

## 📁 Project Structure

```
ExpenseControl/
├── ExpenseControl.Backend/
│   └── ExpenseControl.Api/
│       ├── Entities/
│       ├── Features/
│       │   ├── Transactions/
│       │   ├── Categories/
│       │   └── Balance/
│       ├── Persistence/
│       └── Program.cs
├── ExpenseControl.Frontend/         # Blazor WebAssembly
│   ├── Pages/
│   ├── Components/
│   └── Layout/
└── expense-control-react/           # React Frontend (NEW)
    ├── src/
    │   ├── components/
    │   ├── pages/
    │   ├── services/
    │   └── types/
    └── package.json
```

## 🎯 Core Features

### ✅ Implemented Features

1. **Transaction Management**
   - ✅ Create new income/expense transactions
   - ✅ Edit existing transactions
   - ✅ Delete transactions with confirmation
   - ✅ Filter by date range
   - ✅ Category-based organization

2. **Data Model**
   - **ExpenseEntry Entity**:
     - `Id` (Guid) - Unique identifier
     - `Description` (string) - Expense description
     - `Amount` (decimal) - Expense amount
     - `Date` (DateTime) - Expense date
     - `Category` (string) - Expense category

3. **Backend Features**
   - Vertical Slice Architecture for clean separation
   - FluentValidation for input validation
   - Entity Framework Core for data access
   - OpenTelemetry for observability
   - Prometheus metrics endpoint
   - CORS configuration for frontend integration

4. **Frontend Features**
   - Responsive Bootstrap-based UI
   - Paginated data tables
   - Search and filtering
   - Form validation
   - Delete confirmation modals
   - Navigation menu

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or VS Code
- Modern web browser

### Backend Setup

1. **Navigate to the backend directory**:
   ```bash
   cd /usr/repos/ExpenseControl.Backend/ExpenseControl.Api
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Run the API**:
   ```bash
   dotnet run
   ```

4. **Access the API**:
   - API Base URL: `http://localhost:5293`
   - Swagger UI: `http://localhost:5293/swagger`
   - Prometheus Metrics: `http://localhost:5293/metrics`

### Frontend Setup (Choose one)

#### Option 1: React Frontend (Recommended - Modern Stack)

1. **Navigate to the React frontend directory**:
   ```bash
   cd expense-control-react
   ```

2. **Install dependencies**:
   ```bash
   npm install
   ```

3. **Configure environment** (optional, defaults to localhost:5293):
   ```bash
   echo "VITE_API_URL=http://localhost:5293/api" > .env
   ```

4. **Run the frontend**:
   ```bash
   npm run dev
   ```

5. **Access the application**:
   - Frontend URL: `http://localhost:5173`

See [expense-control-react/README.md](./expense-control-react/README.md) for more details.

#### Option 2: Blazor WebAssembly Frontend (Original)

1. **Navigate to the Blazor frontend directory**:
   ```bash
   cd ExpenseControl.Frontend
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Run the frontend**:
   ```bash
   dotnet run
   ```

4. **Access the application**:
   - Frontend URL: `http://localhost:5171`

## 📋 API Endpoints

### Transaction Management

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/expenses` | Create a new expense entry |
| `GET` | `/api/expenses` | List expense entries with pagination and filtering |
| `DELETE` | `/api/expenses/{id}` | Delete an expense entry by ID |

### Query Parameters for Listing

- `page` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10)
- `description` (string): Filter by description
- `amount` (decimal): Filter by amount
- `date` (DateTime): Filter by date
- `category` (string): Filter by category

## 🏛️ Architecture Patterns

### Backend - Vertical Slice Architecture

Each feature is organized in its own folder with all related components:

```
Features/Expenses/Create/
├── CreateExpenseEntryCommand.cs
├── CreateExpenseEntryHandler.cs
├── CreateExpenseEntryValidator.cs
└── CreateExpenseEntryEndpoint.cs
```

### Frontend - Component-Based Architecture

- **Pages**: Main application views
- **Components**: Reusable UI components
- **Layout**: Navigation and page structure

## 🔧 Configuration

### Backend Configuration

The application uses `appsettings.json` for configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ExpenseControlDb;..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### CORS Configuration

Backend is configured to allow requests from the frontend:
- Allowed Origin: `http://localhost:5171`
- Allowed Methods: Any
- Allowed Headers: Any

## 📊 Observability

### OpenTelemetry Integration

- **Metrics**: Custom counters for expense entries added
- **Tracing**: ASP.NET Core instrumentation
- **Logging**: Console and structured logging
- **Prometheus**: Metrics endpoint for monitoring

### Available Metrics

- `expense_entries_added`: Counter for new expense entries

## 🛠️ Development Guidelines

### Backend Development

1. **Adding New Features**:
   - Create a new folder under `Features/`
   - Follow the Vertical Slice pattern
   - Include Command/Query, Handler, Validator, and Endpoint

2. **Validation**:
   - Use FluentValidation for input validation
   - Register validators in `Program.cs`

3. **Database Changes**:
   - Currently using In-Memory database
   - For production, configure SQL Server connection

### Frontend Development

1. **Adding New Pages**:
   - Create new `.razor` files in `Pages/`
   - Add navigation links in `NavMenu.razor`

2. **Styling**:
   - Use Bootstrap classes for responsive design
   - Custom CSS in component-specific `.css` files

3. **State Management**:
   - Use component state for local data
   - HTTP calls for server communication

## 🔮 Future Enhancements

### Planned Features

- [ ] **Update/Edit** expense entries
- [ ] **Categories Management** (predefined categories)
- [ ] **Dashboard** with charts and analytics
- [ ] **Export functionality** (CSV, PDF)
- [ ] **Budget tracking** and alerts
- [ ] **Date range filtering**
- [ ] **Authentication and authorization**

### Technical Improvements

- [ ] **Real database** (SQL Server, PostgreSQL)
- [ ] **Unit tests** for backend and frontend
- [ ] **Integration tests**
- [ ] **CI/CD pipeline**
- [ ] **Docker containerization**
- [ ] **Better error handling**
- [ ] **Loading states** and user feedback

## 🐛 Troubleshooting

### Common Issues

1. **CORS Errors**:
   - Ensure backend is running on port 5293
   - Check CORS configuration in `Program.cs`

2. **Database Issues**:
   - Currently using In-Memory database
   - Data will be lost on application restart

3. **Port Conflicts**:
   - Backend: Change port in `launchSettings.json`
   - Frontend: Change port in `launchSettings.json`

## 📝 Contributing

1. Follow the existing architecture patterns
2. Add validation for new features
3. Update this README for new features
4. Test thoroughly before submitting changes

## 📄 License

This project is for educational and development purposes.

---

**Last Updated**: December 2024  
**Version**: 1.0.0  
**Status**: Development 