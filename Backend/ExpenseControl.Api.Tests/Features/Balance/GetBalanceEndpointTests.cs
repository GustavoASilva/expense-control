using AutoFixture;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Balance;
using ExpenseControl.Api.Persistence;
using ExpenseControl.Api.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Tests.Features.Balance;

public class GetBalanceEndpointTests
{
    private readonly Fixture _fixture;

    public GetBalanceEndpointTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task GetBalance_WithTransactions_ReturnsCorrectBalance()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var expenseCategory = CreateAndAddCategory(db, "Food", TransactionType.Expense);
        var incomeCategory = CreateAndAddCategory(db, "Salary", TransactionType.Income);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var transactions = new List<Transaction>
        {
            CreateTransaction(household.Id, incomeCategory.Id, 5000m, today, TransactionType.Income),
            CreateTransaction(household.Id, expenseCategory.Id, 1000m, today.AddDays(-1), TransactionType.Expense),
            CreateTransaction(household.Id, expenseCategory.Id, 500m, today.AddDays(-2), TransactionType.Expense)
        };

        db.Transactions.AddRange(transactions);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteGetBalance(db, household.Id, null, null);

        // Assert
        Assert.Equal(5000m, result.Income);
        Assert.Equal(1500m, result.Expenses);
        Assert.Equal(3500m, result.Balance);
        Assert.True(result.HasTransactions);
    }

    [Fact]
    public async Task GetBalance_WithDateRange_ReturnsBalanceForPeriod()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var expenseCategory = CreateAndAddCategory(db, "Food", TransactionType.Expense);
        var incomeCategory = CreateAndAddCategory(db, "Salary", TransactionType.Income);

        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));

        var transactions = new List<Transaction>
        {
            CreateTransaction(household.Id, incomeCategory.Id, 3000m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)), TransactionType.Income),
            CreateTransaction(household.Id, expenseCategory.Id, 500m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-6)), TransactionType.Expense),
            CreateTransaction(household.Id, expenseCategory.Id, 1000m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15)), TransactionType.Expense),
            CreateTransaction(household.Id, incomeCategory.Id, 2000m, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)), TransactionType.Income)
        };

        db.Transactions.AddRange(transactions);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteGetBalance(db, household.Id, startDate, endDate);

        // Assert
        Assert.Equal(3000m, result.Income);
        Assert.Equal(500m, result.Expenses);
        Assert.Equal(2500m, result.Balance);
        Assert.Equal(startDate, result.PeriodStart);
        Assert.Equal(endDate, result.PeriodEnd);
    }

    [Fact]
    public async Task GetBalance_WithNoTransactions_ReturnsZeroBalance()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");

        // Act
        var result = await ExecuteGetBalance(db, household.Id, null, null);

        // Assert
        Assert.Equal(0m, result.Income);
        Assert.Equal(0m, result.Expenses);
        Assert.Equal(0m, result.Balance);
        Assert.False(result.HasTransactions);
    }

    [Fact]
    public async Task GetBalance_WithDefaultDates_UsesLastMonthPeriod()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");

        // Act
        var result = await ExecuteGetBalance(db, household.Id, null, null);

        // Assert
        var expectedStart = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(-1));
        var expectedEnd = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        Assert.Equal(expectedStart, result.PeriodStart);
        Assert.Equal(expectedEnd, result.PeriodEnd);
    }

    [Fact]
    public async Task GetBalance_OnlyExpenses_ReturnsNegativeBalance()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var expenseCategory = CreateAndAddCategory(db, "Food", TransactionType.Expense);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var transactions = new List<Transaction>
        {
            CreateTransaction(household.Id, expenseCategory.Id, 1000m, today, TransactionType.Expense),
            CreateTransaction(household.Id, expenseCategory.Id, 500m, today.AddDays(-1), TransactionType.Expense)
        };

        db.Transactions.AddRange(transactions);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteGetBalance(db, household.Id, null, null);

        // Assert
        Assert.Equal(0m, result.Income);
        Assert.Equal(1500m, result.Expenses);
        Assert.Equal(-1500m, result.Balance);
        Assert.True(result.HasTransactions);
    }

    [Fact]
    public async Task GetBalance_OnlyIncome_ReturnsPositiveBalance()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var incomeCategory = CreateAndAddCategory(db, "Salary", TransactionType.Income);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var transactions = new List<Transaction>
        {
            CreateTransaction(household.Id, incomeCategory.Id, 3000m, today, TransactionType.Income),
            CreateTransaction(household.Id, incomeCategory.Id, 2000m, today.AddDays(-1), TransactionType.Income)
        };

        db.Transactions.AddRange(transactions);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteGetBalance(db, household.Id, null, null);

        // Assert
        Assert.Equal(5000m, result.Income);
        Assert.Equal(0m, result.Expenses);
        Assert.Equal(5000m, result.Balance);
        Assert.True(result.HasTransactions);
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

    private async Task<BalanceResponse> ExecuteGetBalance(
        ExpenseDbContext db,
        Guid householdId,
        DateOnly? startDate,
        DateOnly? endDate)
    {
        var query = db.Transactions
            .Where(t => t.HouseholdId == householdId);

        var periodStart = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(-1));
        var periodEnd = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);

        query = query.Where(t => t.Date >= periodStart && t.Date <= periodEnd);

        var transactions = await query.ToListAsync();

        var income = transactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);
        var expenses = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        return new BalanceResponse(income, expenses, income - expenses, periodStart, periodEnd, transactions.Count > 0);
    }
}
