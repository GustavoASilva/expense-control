using AutoFixture;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Budgets;
using ExpenseControl.Api.Persistence;
using ExpenseControl.Api.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Tests.Features.Budgets;

public class ListBudgetsEndpointTests
{
    private readonly Fixture _fixture;

    public ListBudgetsEndpointTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task ListBudgets_WithNoFilters_ReturnsAllHouseholdBudgets()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var category1 = CreateAndAddCategory(db, "Food", TransactionType.Expense);
        var category2 = CreateAndAddCategory(db, "Transport", TransactionType.Expense);

        var budgets = new List<Budget>
        {
            CreateBudget(household.Id, category1.Id, 1000m, 1, 2024),
            CreateBudget(household.Id, category2.Id, 500m, 1, 2024)
        };

        db.Budgets.AddRange(budgets);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteListBudgets(db, household.Id, null, null, null);

        // Assert
        var okResult = Assert.IsType<Ok<List<BudgetResponse>>>(result);
        Assert.Equal(2, okResult.Value!.Count);
    }

    [Fact]
    public async Task ListBudgets_FilterByYear_ReturnsMatchingBudgets()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var category = CreateAndAddCategory(db, "Food", TransactionType.Expense);

        var budgets = new List<Budget>
        {
            CreateBudget(household.Id, category.Id, 1000m, 1, 2024),
            CreateBudget(household.Id, category.Id, 1200m, 1, 2025)
        };

        db.Budgets.AddRange(budgets);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteListBudgets(db, household.Id, 2024, null, null);

        // Assert
        var okResult = Assert.IsType<Ok<List<BudgetResponse>>>(result);
        Assert.Single(okResult.Value!);
        Assert.Equal(2024, okResult.Value[0].Year);
    }

    [Fact]
    public async Task ListBudgets_FilterByMonth_ReturnsMatchingBudgets()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var category = CreateAndAddCategory(db, "Food", TransactionType.Expense);

        var budgets = new List<Budget>
        {
            CreateBudget(household.Id, category.Id, 1000m, 1, 2024),
            CreateBudget(household.Id, category.Id, 1200m, 2, 2024)
        };

        db.Budgets.AddRange(budgets);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteListBudgets(db, household.Id, null, 1, null);

        // Assert
        var okResult = Assert.IsType<Ok<List<BudgetResponse>>>(result);
        Assert.Single(okResult.Value!);
        Assert.Equal(1, okResult.Value[0].Month);
    }

    [Fact]
    public async Task ListBudgets_FilterByCategory_ReturnsMatchingBudgets()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var category1 = CreateAndAddCategory(db, "Food", TransactionType.Expense);
        var category2 = CreateAndAddCategory(db, "Transport", TransactionType.Expense);

        var budgets = new List<Budget>
        {
            CreateBudget(household.Id, category1.Id, 1000m, 1, 2024),
            CreateBudget(household.Id, category2.Id, 500m, 1, 2024)
        };

        db.Budgets.AddRange(budgets);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteListBudgets(db, household.Id, null, null, category1.Id);

        // Assert
        var okResult = Assert.IsType<Ok<List<BudgetResponse>>>(result);
        Assert.Single(okResult.Value!);
        Assert.Equal(category1.Id, okResult.Value[0].CategoryId);
    }

    [Fact]
    public async Task ListBudgets_IncludesCategoryDetails()
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
        var result = await ExecuteListBudgets(db, household.Id, null, null, null);

        // Assert
        var okResult = Assert.IsType<Ok<List<BudgetResponse>>>(result);
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

    private async Task<IResult> ExecuteListBudgets(ExpenseDbContext db, Guid householdId, int? year, int? month, Guid? categoryId)
    {
        var query = db.Budgets.Include(b => b.Category)
            .Where(b => b.HouseholdId == householdId);
        if (year.HasValue) query = query.Where(b => b.Year == year);
        if (month.HasValue) query = query.Where(b => b.Month == month);
        if (categoryId.HasValue) query = query.Where(b => b.CategoryId == categoryId);
        var budgets = await query.ToListAsync();
        return Results.Ok(budgets.ToResponse());
    }
}
