using AutoFixture;
using AutoFixture.Xunit2;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Transactions.Create;
using ExpenseControl.Api.Persistence;
using ExpenseControl.Api.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ExpenseControl.Api.Tests.Features.Transactions;

public class CreateTransactionEndpointTests
{
    private readonly Fixture _fixture;

    public CreateTransactionEndpointTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task CreateTransaction_WithExpenseType_CreatesExpenseTransaction()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = _fixture.Build<Household>()
            .With(h => h.Id, Guid.NewGuid())
            .With(h => h.Name, "Test Household")
            .Create();

        var category = _fixture.Build<Category>()
            .With(c => c.Id, Guid.NewGuid())
            .With(c => c.Name, "Food")
            .With(c => c.Type, TransactionType.Expense)
            .Create();

        db.Households.Add(household);
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var command = new CreateTransactionCommand(
            Description: "Grocery shopping",
            Amount: 150.50m,
            Date: DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId: category.Id,
            Type: TransactionType.Expense,
            Notes: "Weekly groceries",
            HouseholdId: household.Id
        );

        // Act
        var result = await ExecuteCreateTransaction(db, command);

        // Assert
        var createdResult = Assert.IsType<Created<Transaction>>(result);
        Assert.NotNull(createdResult.Value);
        Assert.IsType<Expense>(createdResult.Value);
        Assert.Equal(command.Description, createdResult.Value.Description);
        Assert.Equal(command.Amount, createdResult.Value.Amount);
        Assert.Equal(command.CategoryId, createdResult.Value.CategoryId);
    }

    [Fact]
    public async Task CreateTransaction_WithIncomeType_CreatesIncomeTransaction()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = _fixture.Build<Household>()
            .With(h => h.Id, Guid.NewGuid())
            .With(h => h.Name, "Test Household")
            .Create();

        var category = _fixture.Build<Category>()
            .With(c => c.Id, Guid.NewGuid())
            .With(c => c.Name, "Salary")
            .With(c => c.Type, TransactionType.Income)
            .Create();

        db.Households.Add(household);
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var command = new CreateTransactionCommand(
            Description: "Monthly salary",
            Amount: 5000m,
            Date: DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId: category.Id,
            Type: TransactionType.Income,
            Notes: "Salary payment",
            HouseholdId: household.Id
        );

        // Act
        var result = await ExecuteCreateTransaction(db, command);

        // Assert
        var createdResult = Assert.IsType<Created<Transaction>>(result);
        Assert.NotNull(createdResult.Value);
        Assert.IsType<Income>(createdResult.Value);
        Assert.Equal(command.Description, createdResult.Value.Description);
        Assert.Equal(command.Amount, createdResult.Value.Amount);
    }

    [Fact]
    public async Task CreateTransaction_WithNonExistentCategory_ReturnsNotFound()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = _fixture.Build<Household>()
            .With(h => h.Id, Guid.NewGuid())
            .With(h => h.Name, "Test Household")
            .Create();

        db.Households.Add(household);
        await db.SaveChangesAsync();

        var command = new CreateTransactionCommand(
            Description: "Test transaction",
            Amount: 100m,
            Date: DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId: Guid.NewGuid(), // Non-existent category
            Type: TransactionType.Expense,
            Notes: null,
            HouseholdId: household.Id
        );

        // Act
        var result = await ExecuteCreateTransaction(db, command);

        // Assert
        var notFoundResult = Assert.IsType<NotFound<string>>(result);
        Assert.Equal("Category not found", notFoundResult.Value);
    }

    [Fact]
    public async Task CreateTransaction_WithNonExistentHousehold_ReturnsNotFound()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var category = _fixture.Build<Category>()
            .With(c => c.Id, Guid.NewGuid())
            .With(c => c.Name, "Food")
            .With(c => c.Type, TransactionType.Expense)
            .Create();

        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var command = new CreateTransactionCommand(
            Description: "Test transaction",
            Amount: 100m,
            Date: DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId: category.Id,
            Type: TransactionType.Expense,
            Notes: null,
            HouseholdId: Guid.NewGuid() // Non-existent household
        );

        // Act
        var result = await ExecuteCreateTransaction(db, command);

        // Assert
        var notFoundResult = Assert.IsType<NotFound<string>>(result);
        Assert.Equal("Household not found", notFoundResult.Value);
    }

    [Theory]
    [InlineAutoData(0)]
    [InlineAutoData(50.99)]
    [InlineAutoData(1000.50)]
    public async Task CreateTransaction_WithVariousAmounts_CreatesSuccessfully(decimal amount)
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = _fixture.Build<Household>()
            .With(h => h.Id, Guid.NewGuid())
            .With(h => h.Name, "Test Household")
            .Create();

        var category = _fixture.Build<Category>()
            .With(c => c.Id, Guid.NewGuid())
            .With(c => c.Name, "Test Category")
            .With(c => c.Type, TransactionType.Expense)
            .Create();

        db.Households.Add(household);
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var command = new CreateTransactionCommand(
            Description: "Test transaction",
            Amount: amount,
            Date: DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId: category.Id,
            Type: TransactionType.Expense,
            Notes: null,
            HouseholdId: household.Id
        );

        // Act
        var result = await ExecuteCreateTransaction(db, command);

        // Assert
        var createdResult = Assert.IsType<Created<Transaction>>(result);
        Assert.Equal(amount, createdResult.Value!.Amount);
    }

    private async Task<IResult> ExecuteCreateTransaction(ExpenseDbContext db, CreateTransactionCommand command)
    {
        var category = await db.Categories.FindAsync(command.CategoryId);
        if (category == null)
            return Results.NotFound("Category not found");

        var household = await db.Households.FindAsync(command.HouseholdId);
        if (household == null)
            return Results.NotFound("Household not found");

        Transaction transaction = command.Type switch
        {
            TransactionType.Expense => new Expense(),
            TransactionType.Income => new Income(),
            _ => throw new ArgumentException("Invalid transaction type")
        };

        transaction.Id = Guid.NewGuid();
        transaction.Description = command.Description;
        transaction.Amount = command.Amount;
        transaction.Date = command.Date;
        transaction.CategoryId = command.CategoryId;
        transaction.Notes = command.Notes ?? string.Empty;
        transaction.HouseholdId = command.HouseholdId;

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync();

        return Results.Created($"/api/transactions/{transaction.Id}", transaction);
    }
}
