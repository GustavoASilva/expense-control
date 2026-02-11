using AutoFixture;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Persistence;
using ExpenseControl.Api.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Tests.Features.Budgets;

public class CreateBudgetEndpointTests
{
    private readonly Fixture _fixture;

    public CreateBudgetEndpointTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task CreateBudget_WithNewBudget_CreatesBudget()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var category = CreateAndAddCategory(db, "Food", TransactionType.Expense);

        var budget = new Budget
        {
            CategoryId = category.Id,
            Amount = 1000m,
            Month = 1,
            Year = 2024,
            HouseholdId = household.Id
        };

        // Act
        var result = await ExecuteCreateBudget(db, budget);

        // Assert
        var okResult = Assert.IsType<Ok<Budget>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(budget.Amount, okResult.Value.Amount);
        Assert.Equal(budget.Month, okResult.Value.Month);
        Assert.Equal(budget.Year, okResult.Value.Year);
        Assert.NotEqual(Guid.Empty, okResult.Value.Id);
    }

    [Fact]
    public async Task CreateBudget_WithExistingBudget_UpdatesAmount()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var category = CreateAndAddCategory(db, "Food", TransactionType.Expense);

        var existingBudget = new Budget
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Amount = 1000m,
            Month = 1,
            Year = 2024,
            HouseholdId = household.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Budgets.Add(existingBudget);
        await db.SaveChangesAsync();

        var updatedBudget = new Budget
        {
            CategoryId = category.Id,
            Amount = 1500m,
            Month = 1,
            Year = 2024,
            HouseholdId = household.Id
        };

        // Act
        var result = await ExecuteCreateBudget(db, updatedBudget);

        // Assert
        var okResult = Assert.IsType<Ok<Budget>>(result);
        Assert.Equal(1500m, okResult.Value!.Amount);
        
        var budgetInDb = await db.Budgets.FirstOrDefaultAsync(b => b.Id == existingBudget.Id);
        Assert.NotNull(budgetInDb);
        Assert.Equal(1500m, budgetInDb.Amount);
    }

    [Fact]
    public async Task CreateBudget_WithNonExistentHousehold_ReturnsNotFound()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var category = CreateAndAddCategory(db, "Food", TransactionType.Expense);

        var budget = new Budget
        {
            CategoryId = category.Id,
            Amount = 1000m,
            Month = 1,
            Year = 2024,
            HouseholdId = Guid.NewGuid()
        };

        // Act
        var result = await ExecuteCreateBudget(db, budget);

        // Assert
        var notFoundResult = Assert.IsType<NotFound<string>>(result);
        Assert.Equal("Household not found", notFoundResult.Value);
    }

    [Fact]
    public async Task CreateBudget_SetsCreatedAndUpdatedTimestamps()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var category = CreateAndAddCategory(db, "Food", TransactionType.Expense);

        var budget = new Budget
        {
            CategoryId = category.Id,
            Amount = 1000m,
            Month = 1,
            Year = 2024,
            HouseholdId = household.Id
        };

        var beforeCreate = DateTime.UtcNow;

        // Act
        var result = await ExecuteCreateBudget(db, budget);

        var afterCreate = DateTime.UtcNow;

        // Assert
        var okResult = Assert.IsType<Ok<Budget>>(result);
        Assert.True(okResult.Value!.CreatedAt >= beforeCreate && okResult.Value.CreatedAt <= afterCreate);
        Assert.True(okResult.Value.UpdatedAt >= beforeCreate && okResult.Value.UpdatedAt <= afterCreate);
    }

    [Fact]
    public async Task CreateBudget_UpdateExisting_OnlyUpdatesUpdatedTimestamp()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var category = CreateAndAddCategory(db, "Food", TransactionType.Expense);

        var existingBudget = new Budget
        {
            Id = Guid.NewGuid(),
            CategoryId = category.Id,
            Amount = 1000m,
            Month = 1,
            Year = 2024,
            HouseholdId = household.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        db.Budgets.Add(existingBudget);
        await db.SaveChangesAsync();

        var originalCreatedAt = existingBudget.CreatedAt;
        var updatedBudget = new Budget
        {
            CategoryId = category.Id,
            Amount = 1500m,
            Month = 1,
            Year = 2024,
            HouseholdId = household.Id
        };

        await Task.Delay(100); // Ensure time difference

        // Act
        var result = await ExecuteCreateBudget(db, updatedBudget);

        // Assert
        var budgetInDb = await db.Budgets.FirstOrDefaultAsync(b => b.Id == existingBudget.Id);
        Assert.NotNull(budgetInDb);
        Assert.Equal(originalCreatedAt, budgetInDb.CreatedAt);
        Assert.True(budgetInDb.UpdatedAt > originalCreatedAt);
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

    private async Task<IResult> ExecuteCreateBudget(ExpenseDbContext db, Budget budget)
    {
        var household = await db.Households.FindAsync(budget.HouseholdId);
        if (household == null)
            return Results.NotFound("Household not found");

        var existing = await db.Budgets.FirstOrDefaultAsync(b =>
            b.CategoryId == budget.CategoryId &&
            b.Month == budget.Month &&
            b.Year == budget.Year &&
            b.HouseholdId == budget.HouseholdId);

        if (existing != null)
        {
            existing.Amount = budget.Amount;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            budget.Id = Guid.NewGuid();
            budget.CreatedAt = DateTime.UtcNow;
            budget.UpdatedAt = DateTime.UtcNow;
            db.Budgets.Add(budget);
        }
        await db.SaveChangesAsync();
        return Results.Ok(budget);
    }
}
