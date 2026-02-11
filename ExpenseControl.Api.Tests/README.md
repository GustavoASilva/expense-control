# ExpenseControl.Api.Tests

Comprehensive unit test suite for the ExpenseControl API using xUnit, Moq, and AutoFixture.

## Test Framework

- **xUnit**: Testing framework
- **Moq**: Mocking framework (available but not heavily used - most tests use in-memory database)
- **AutoFixture**: Test data generation
- **EF Core InMemory**: In-memory database for integration testing

## Running Tests

### Run all tests
```bash
dotnet test
```

### Run tests with detailed output
```bash
dotnet test --verbosity normal
```

### Run tests from the test project directory
```bash
cd ExpenseControl.Api.Tests
dotnet test
```

## Test Coverage

The test suite covers:

### Transaction Endpoints
- Creating transactions (expenses and income)
- Listing transactions with various filters
- Getting individual transactions
- Updating transactions
- Deleting transactions
- Household isolation

### Balance Endpoints
- Overall balance calculation
- Balance by category
- Monthly balance summaries
- Date range filtering
- Income vs expense tracking

### Budget Endpoints
- Creating and updating budgets
- Listing budgets with filters
- Budget usage calculation
- Percentage tracking
- Over-budget scenarios

### Category Endpoints
- Listing categories with type filtering
- Getting individual categories
- Category properties validation

### Household Endpoints
- Creating households
- Listing households with ordering
- Getting individual households
- Timestamp validation

## Test Patterns

### In-Memory Database
Tests use EF Core's in-memory database provider for fast, isolated testing:

```csharp
var dbName = Guid.NewGuid().ToString();
await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);
```

### AutoFixture for Test Data
AutoFixture generates test data with specific properties:

```csharp
var household = _fixture.Build<Household>()
    .With(h => h.Id, Guid.NewGuid())
    .With(h => h.Name, "Test Household")
    .Create();
```

### Arrange-Act-Assert Pattern
All tests follow the AAA pattern for clarity:

```csharp
// Arrange - Set up test data and context
// Act - Execute the code under test
// Assert - Verify the results
```

## Key Test Scenarios

### Security & Isolation
- Tests verify that households can only access their own data
- Cross-household data access returns NotFound

### Business Logic
- Balance calculations (income - expenses)
- Budget usage percentages
- Date range filtering
- Transaction type filtering

### Edge Cases
- Empty result sets
- Non-existent entities
- Over-budget scenarios
- Invalid date ranges

## Continuous Integration

These tests are designed to run in CI/CD pipelines:
- Fast execution
- No external dependencies
- Deterministic results
- Isolated test runs

## Contributing

When adding new features:
1. Create corresponding test files in the appropriate feature folder
2. Follow the existing naming conventions (e.g., `{Feature}EndpointTests.cs`)
3. Use the helper methods for creating test data
4. Ensure tests are isolated and don't depend on execution order
5. Run all tests to verify no regressions
