using AutoFixture;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Transactions;
using ExpenseControl.Api.Persistence;
using ExpenseControl.Api.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Tests.Features.Transactions;

public class ListTransactionsEndpointTests
{
    private readonly Fixture _fixture;

    public ListTransactionsEndpointTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task ListTransactions_WithNoFilters_ReturnsAllHouseholdTransactions()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var category = CreateAndAddCategory(db, "Food", TransactionType.Expense);

        var transactions = new List<Transaction>
        {
            CreateTransaction(household.Id, category.Id, 100m, DateOnly.FromDateTime(DateTime.UtcNow), TransactionType.Expense),
            CreateTransaction(household.Id, category.Id, 200m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), TransactionType.Expense),
            CreateTransaction(household.Id, category.Id, 300m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), TransactionType.Expense)
        };

        db.Transactions.AddRange(transactions);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteListTransactions(db, household.Id, null, null, null, null, null, null);

        // Assert
        var okResult = Assert.IsType<Ok<List<TransactionResponse>>>(result);
        Assert.Equal(3, okResult.Value!.Count);
    }

    [Fact]
    public async Task ListTransactions_FilterByType_ReturnsMatchingTransactions()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var expenseCategory = CreateAndAddCategory(db, "Food", TransactionType.Expense);
        var incomeCategory = CreateAndAddCategory(db, "Salary", TransactionType.Income);

        var transactions = new List<Transaction>
        {
            CreateTransaction(household.Id, expenseCategory.Id, 100m, DateOnly.FromDateTime(DateTime.UtcNow), TransactionType.Expense),
            CreateTransaction(household.Id, expenseCategory.Id, 200m, DateOnly.FromDateTime(DateTime.UtcNow), TransactionType.Expense),
            CreateTransaction(household.Id, incomeCategory.Id, 5000m, DateOnly.FromDateTime(DateTime.UtcNow), TransactionType.Income)
        };

        db.Transactions.AddRange(transactions);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteListTransactions(db, household.Id, TransactionType.Expense, null, null, null, null, null);

        // Assert
        var okResult = Assert.IsType<Ok<List<TransactionResponse>>>(result);
        Assert.Equal(2, okResult.Value!.Count);
        Assert.All(okResult.Value, t => Assert.Equal(TransactionType.Expense, t.Type));
    }

    [Fact]
    public async Task ListTransactions_FilterByCategory_ReturnsMatchingTransactions()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var foodCategory = CreateAndAddCategory(db, "Food", TransactionType.Expense);
        var transportCategory = CreateAndAddCategory(db, "Transport", TransactionType.Expense);

        var transactions = new List<Transaction>
        {
            CreateTransaction(household.Id, foodCategory.Id, 100m, DateOnly.FromDateTime(DateTime.UtcNow), TransactionType.Expense),
            CreateTransaction(household.Id, foodCategory.Id, 200m, DateOnly.FromDateTime(DateTime.UtcNow), TransactionType.Expense),
            CreateTransaction(household.Id, transportCategory.Id, 50m, DateOnly.FromDateTime(DateTime.UtcNow), TransactionType.Expense)
        };

        db.Transactions.AddRange(transactions);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteListTransactions(db, household.Id, null, foodCategory.Id, null, null, null, null);

        // Assert
        var okResult = Assert.IsType<Ok<List<TransactionResponse>>>(result);
        Assert.Equal(2, okResult.Value!.Count);
        Assert.All(okResult.Value, t => Assert.Equal(foodCategory.Id, t.CategoryId));
    }

    [Fact]
    public async Task ListTransactions_FilterByDateRange_ReturnsMatchingTransactions()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var category = CreateAndAddCategory(db, "Food", TransactionType.Expense);

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));

        var transactions = new List<Transaction>
        {
            CreateTransaction(household.Id, category.Id, 100m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)), TransactionType.Expense),
            CreateTransaction(household.Id, category.Id, 200m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15)), TransactionType.Expense),
            CreateTransaction(household.Id, category.Id, 300m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)), TransactionType.Expense)
        };

        db.Transactions.AddRange(transactions);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteListTransactions(db, household.Id, null, null, startDate, endDate, null, null);

        // Assert
        var okResult = Assert.IsType<Ok<List<TransactionResponse>>>(result);
        Assert.Single(okResult.Value!);
    }

    [Fact]
    public async Task ListTransactions_WithLimitAndOffset_ReturnsPaginatedResults()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var category = CreateAndAddCategory(db, "Food", TransactionType.Expense);

        var transactions = new List<Transaction>();
        for (int i = 0; i < 10; i++)
        {
            transactions.Add(CreateTransaction(household.Id, category.Id, 100m + i, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-i)), TransactionType.Expense));
        }

        db.Transactions.AddRange(transactions);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteListTransactions(db, household.Id, null, null, null, null, limit: 5, offset: 2);

        // Assert
        var okResult = Assert.IsType<Ok<List<TransactionResponse>>>(result);
        Assert.Equal(5, okResult.Value!.Count);
    }

    [Fact]
    public async Task ListTransactions_OrdersByDateDescending()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var category = CreateAndAddCategory(db, "Food", TransactionType.Expense);

        var oldestDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));
        var middleDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));
        var newestDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var transactions = new List<Transaction>
        {
            CreateTransaction(household.Id, category.Id, 100m, oldestDate, TransactionType.Expense),
            CreateTransaction(household.Id, category.Id, 200m, newestDate, TransactionType.Expense),
            CreateTransaction(household.Id, category.Id, 300m, middleDate, TransactionType.Expense)
        };

        db.Transactions.AddRange(transactions);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteListTransactions(db, household.Id, null, null, null, null, null, null);

        // Assert
        var okResult = Assert.IsType<Ok<List<TransactionResponse>>>(result);
        Assert.Equal(newestDate, okResult.Value![0].Date);
        Assert.Equal(middleDate, okResult.Value[1].Date);
        Assert.Equal(oldestDate, okResult.Value[2].Date);
    }

    [Fact]
    public async Task ListTransactions_IncludesCategoryInformation()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var category = CreateAndAddCategory(db, "Food", TransactionType.Expense);

        var transaction = CreateTransaction(household.Id, category.Id, 100m, DateOnly.FromDateTime(DateTime.UtcNow), TransactionType.Expense);

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteListTransactions(db, household.Id, null, null, null, null, null, null);

        // Assert
        var okResult = Assert.IsType<Ok<List<TransactionResponse>>>(result);
        Assert.NotNull(okResult.Value![0].Category);
        Assert.Equal("Food", okResult.Value[0].Category!.Name);
    }

    private Household CreateAndAddHousehold(ExpenseDbContext db, string name)
    {
        var household = _fixture.Build<Household>()
            .With(h => h.Id, Guid.NewGuid())
            .With(h => h.Name, name)
            .Create();
        db.Households.Add(household);
        db.SaveChanges();
        return household;
    }

    private Category CreateAndAddCategory(ExpenseDbContext db, string name, TransactionType type)
    {
        var category = _fixture.Build<Category>()
            .With(c => c.Id, Guid.NewGuid())
            .With(c => c.Name, name)
            .With(c => c.Type, type)
            .Create();
        db.Categories.Add(category);
        db.SaveChanges();
        return category;
    }

    private Transaction CreateTransaction(Guid householdId, Guid categoryId, decimal amount, DateOnly date, TransactionType type)
    {
        Transaction transaction = type switch
        {
            TransactionType.Expense => new Expense(),
            TransactionType.Income => new Income(),
            _ => throw new ArgumentException("Invalid transaction type")
        };

        transaction.Id = Guid.NewGuid();
        transaction.Description = $"Transaction {amount}";
        transaction.Amount = amount;
        transaction.Date = date;
        transaction.CategoryId = categoryId;
        transaction.Notes = "Test notes";
        transaction.HouseholdId = householdId;

        return transaction;
    }

    private async Task<IResult> ExecuteListTransactions(
        ExpenseDbContext db,
        Guid householdId,
        TransactionType? type,
        Guid? categoryId,
        DateOnly? startDate,
        DateOnly? endDate,
        int? limit,
        int? offset)
    {
        var query = db.Transactions
            .Include(t => t.Category)
            .Where(t => t.HouseholdId == householdId);

        if (type.HasValue)
            query = query.Where(t => t.Type == type.Value);

        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);

        if (startDate.HasValue)
            query = query.Where(t => t.Date >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.Date <= endDate.Value);

        query = query.OrderByDescending(t => t.Date);

        if (offset.HasValue)
            query = query.Skip(offset.Value);

        if (limit.HasValue)
            query = query.Take(limit.Value);

        var transactions = await query.ToListAsync();
        return Results.Ok(transactions.ToResponse());
    }
}
