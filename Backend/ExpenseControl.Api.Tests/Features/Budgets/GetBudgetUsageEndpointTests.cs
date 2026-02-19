using AutoFixture;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Budgets.Usage;
using ExpenseControl.Api.Persistence;
using ExpenseControl.Api.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Tests.Features.Budgets;

public class GetBudgetUsageEndpointTests
{
    private readonly Fixture _fixture;

    public GetBudgetUsageEndpointTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task GetBudgetUsage_WithBudgetAndTransactions_ReturnsCorrectUsage()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var category = CreateAndAddCategory(db, "Food", TransactionType.Expense);

        var budget = CreateBudget(household.Id, category.Id, 1000m, 1, 2024);
        db.Budgets.Add(budget);

        var transactions = new List<Transaction>
        {
            CreateTransaction(household.Id, category.Id, 200m, new DateOnly(2024, 1, 15), TransactionType.Expense),
            CreateTransaction(household.Id, category.Id, 300m, new DateOnly(2024, 1, 20), TransactionType.Expense)
        };

        db.Transactions.AddRange(transactions);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteGetBudgetUsage(db, household.Id, category.Id, 2024, 1);

        // Assert
        var okResult = Assert.IsType<Ok<BudgetUsageResponse>>(result);
        Assert.Equal(1000m, okResult.Value!.Amount);
        Assert.Equal(500m, okResult.Value.Usage);
        Assert.Equal(50m, okResult.Value.Percent);
    }

    [Fact]
    public async Task GetBudgetUsage_WithNoBudget_ReturnsNotFound()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var category = CreateAndAddCategory(db, "Food", TransactionType.Expense);

        // Act
        var result = await ExecuteGetBudgetUsage(db, household.Id, category.Id, 2024, 1);

        // Assert
        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task GetBudgetUsage_WithNoTransactions_ReturnsZeroUsage()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var category = CreateAndAddCategory(db, "Food", TransactionType.Expense);

        var budget = CreateBudget(household.Id, category.Id, 1000m, 1, 2024);
        db.Budgets.Add(budget);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteGetBudgetUsage(db, household.Id, category.Id, 2024, 1);

        // Assert
        var okResult = Assert.IsType<Ok<BudgetUsageResponse>>(result);
        Assert.Equal(1000m, okResult.Value!.Amount);
        Assert.Equal(0m, okResult.Value.Usage);
        Assert.Equal(0m, okResult.Value.Percent);
    }

    [Fact]
    public async Task GetBudgetUsage_OverBudget_ReturnsPercentageOver100()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var category = CreateAndAddCategory(db, "Food", TransactionType.Expense);

        var budget = CreateBudget(household.Id, category.Id, 1000m, 1, 2024);
        db.Budgets.Add(budget);

        var transactions = new List<Transaction>
        {
            CreateTransaction(household.Id, category.Id, 800m, new DateOnly(2024, 1, 15), TransactionType.Expense),
            CreateTransaction(household.Id, category.Id, 500m, new DateOnly(2024, 1, 20), TransactionType.Expense)
        };

        db.Transactions.AddRange(transactions);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteGetBudgetUsage(db, household.Id, category.Id, 2024, 1);

        // Assert
        var okResult = Assert.IsType<Ok<BudgetUsageResponse>>(result);
        Assert.Equal(1300m, okResult.Value!.Usage);
        Assert.Equal(130m, okResult.Value.Percent);
    }

    [Fact]
    public async Task GetBudgetUsage_OnlyCountsExpensesNotIncome()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var category = CreateAndAddCategory(db, "Food", TransactionType.Expense);

        var budget = CreateBudget(household.Id, category.Id, 1000m, 1, 2024);
        db.Budgets.Add(budget);

        var transactions = new List<Transaction>
        {
            CreateTransaction(household.Id, category.Id, 200m, new DateOnly(2024, 1, 15), TransactionType.Expense),
            CreateTransaction(household.Id, category.Id, 1000m, new DateOnly(2024, 1, 20), TransactionType.Income) // Should be ignored
        };

        db.Transactions.AddRange(transactions);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteGetBudgetUsage(db, household.Id, category.Id, 2024, 1);

        // Assert
        var okResult = Assert.IsType<Ok<BudgetUsageResponse>>(result);
        Assert.Equal(200m, okResult.Value!.Usage);
        Assert.Equal(20m, okResult.Value.Percent);
    }

    [Fact]
    public async Task GetBudgetUsage_OnlyCountsMatchingMonthAndYear()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var category = CreateAndAddCategory(db, "Food", TransactionType.Expense);

        var budget = CreateBudget(household.Id, category.Id, 1000m, 1, 2024);
        db.Budgets.Add(budget);

        var transactions = new List<Transaction>
        {
            CreateTransaction(household.Id, category.Id, 200m, new DateOnly(2024, 1, 15), TransactionType.Expense),
            CreateTransaction(household.Id, category.Id, 300m, new DateOnly(2024, 2, 15), TransactionType.Expense), // Different month
            CreateTransaction(household.Id, category.Id, 400m, new DateOnly(2025, 1, 15), TransactionType.Expense) // Different year
        };

        db.Transactions.AddRange(transactions);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteGetBudgetUsage(db, household.Id, category.Id, 2024, 1);

        // Assert
        var okResult = Assert.IsType<Ok<BudgetUsageResponse>>(result);
        Assert.Equal(200m, okResult.Value!.Usage);
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

    private Budget CreateBudget(Guid householdId, Guid categoryId, decimal amount, int month, int year)
    {
        return new Budget
        {
            Id = Guid.NewGuid(),
            HouseholdId = householdId,
            CategoryId = categoryId,
            Amount = amount,
            Month = month,
            Year = year,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
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

    private async Task<IResult> ExecuteGetBudgetUsage(ExpenseDbContext db, Guid householdId, Guid categoryId, int year, int month)
    {
        var budget = await db.Budgets.FirstOrDefaultAsync(b =>
            b.CategoryId == categoryId &&
            b.Year == year &&
            b.Month == month &&
            b.HouseholdId == householdId);
        if (budget == null) return Results.NotFound();

        var usage = await db.Transactions
            .Where(t => t.CategoryId == categoryId &&
                t.Date.Year == year &&
                t.Date.Month == month &&
                t.Type == TransactionType.Expense &&
                t.HouseholdId == householdId)
            .SumAsync(t => t.Amount);
        var percent = budget.Amount > 0 ? (usage / budget.Amount) * 100m : 0m;
        return Results.Ok(new BudgetUsageResponse(budget.Amount, usage, percent));
    }
}
