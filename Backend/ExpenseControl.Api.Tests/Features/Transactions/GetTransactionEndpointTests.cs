using AutoFixture;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Persistence;
using ExpenseControl.Api.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Tests.Features.Transactions;

public class GetTransactionEndpointTests
{
    private readonly Fixture _fixture;

    public GetTransactionEndpointTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task GetTransaction_WithValidId_ReturnsTransaction()
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

        var transaction = new Expense
        {
            Id = Guid.NewGuid(),
            Description = "Test transaction",
            Amount = 100m,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId = category.Id,
            Notes = "Test notes",
            HouseholdId = household.Id
        };

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteGetTransaction(db, transaction.Id, household.Id);

        // Assert
        var okResult = Assert.IsType<Ok<Transaction>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(transaction.Id, okResult.Value.Id);
        Assert.Equal(transaction.Description, okResult.Value.Description);
        Assert.Equal(transaction.Amount, okResult.Value.Amount);
        Assert.NotNull(okResult.Value.Category);
    }

    [Fact]
    public async Task GetTransaction_WithNonExistentId_ReturnsNotFound()
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

        // Act
        var result = await ExecuteGetTransaction(db, Guid.NewGuid(), household.Id);

        // Assert
        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task GetTransaction_WithWrongHouseholdId_ReturnsNotFound()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household1 = _fixture.Build<Household>()
            .With(h => h.Id, Guid.NewGuid())
            .With(h => h.Name, "Household 1")
            .Create();

        var household2 = _fixture.Build<Household>()
            .With(h => h.Id, Guid.NewGuid())
            .With(h => h.Name, "Household 2")
            .Create();

        var category = _fixture.Build<Category>()
            .With(c => c.Id, Guid.NewGuid())
            .With(c => c.Name, "Food")
            .With(c => c.Type, TransactionType.Expense)
            .Create();

        db.Households.AddRange(household1, household2);
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var transaction = new Expense
        {
            Id = Guid.NewGuid(),
            Description = "Test transaction",
            Amount = 100m,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId = category.Id,
            Notes = "Test notes",
            HouseholdId = household1.Id
        };

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync();

        // Act - Try to get transaction with wrong household ID
        var result = await ExecuteGetTransaction(db, transaction.Id, household2.Id);

        // Assert
        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task GetTransaction_IncludesCategoryDetails()
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
            .With(c => c.Description, "Food and groceries")
            .With(c => c.Type, TransactionType.Expense)
            .With(c => c.IconName, "food_icon")
            .Create();

        db.Households.Add(household);
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var transaction = new Expense
        {
            Id = Guid.NewGuid(),
            Description = "Grocery shopping",
            Amount = 150m,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId = category.Id,
            Notes = "Weekly shopping",
            HouseholdId = household.Id
        };

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteGetTransaction(db, transaction.Id, household.Id);

        // Assert
        var okResult = Assert.IsType<Ok<Transaction>>(result);
        Assert.NotNull(okResult.Value!.Category);
        Assert.Equal("Food", okResult.Value.Category!.Name);
        Assert.Equal("Food and groceries", okResult.Value.Category.Description);
        Assert.Equal("food_icon", okResult.Value.Category.IconName);
    }

    private async Task<IResult> ExecuteGetTransaction(ExpenseDbContext db, Guid id, Guid householdId)
    {
        var transaction = await db.Transactions
            .Include(t => t.Category)
            .Where(t => t.HouseholdId == householdId)
            .FirstOrDefaultAsync(t => t.Id == id);

        return transaction is null ? Results.NotFound() : Results.Ok(transaction);
    }
}
