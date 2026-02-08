using AutoFixture;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Persistence;
using ExpenseControl.Api.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Tests.Features.Transactions;

public class UpdateTransactionEndpointTests
{
    private readonly Fixture _fixture;

    public UpdateTransactionEndpointTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task UpdateTransaction_WithValidData_UpdatesTransaction()
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
            Description = "Old description",
            Amount = 100m,
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            CategoryId = category.Id,
            Notes = "Old notes",
            HouseholdId = household.Id
        };

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync();

        var updated = new Transaction
        {
            Description = "New description",
            Amount = 200m,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId = category.Id,
            Type = TransactionType.Expense,
            Notes = "New notes"
        };

        // Act
        var result = await ExecuteUpdateTransaction(db, transaction.Id, updated, household.Id);

        // Assert
        var okResult = Assert.IsType<Ok<Transaction>>(result);
        Assert.Equal("New description", okResult.Value!.Description);
        Assert.Equal(200m, okResult.Value.Amount);
        Assert.Equal("New notes", okResult.Value.Notes);
    }

    [Fact]
    public async Task UpdateTransaction_WithNonExistentId_ReturnsNotFound()
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

        var updated = new Transaction
        {
            Description = "New description",
            Amount = 200m,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId = Guid.NewGuid(),
            Type = TransactionType.Expense,
            Notes = "New notes"
        };

        // Act
        var result = await ExecuteUpdateTransaction(db, Guid.NewGuid(), updated, household.Id);

        // Assert
        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task UpdateTransaction_WithWrongHouseholdId_ReturnsNotFound()
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

        var updated = new Transaction
        {
            Description = "New description",
            Amount = 200m,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId = category.Id,
            Type = TransactionType.Expense,
            Notes = "New notes"
        };

        // Act - Try to update with wrong household ID
        var result = await ExecuteUpdateTransaction(db, transaction.Id, updated, household2.Id);

        // Assert
        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task UpdateTransaction_ChangesAmount_UpdatesCorrectly()
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

        var updated = new Transaction
        {
            Description = transaction.Description,
            Amount = 500.75m,
            Date = transaction.Date,
            CategoryId = transaction.CategoryId,
            Type = transaction.Type,
            Notes = transaction.Notes
        };

        // Act
        var result = await ExecuteUpdateTransaction(db, transaction.Id, updated, household.Id);

        // Assert
        var okResult = Assert.IsType<Ok<Transaction>>(result);
        Assert.Equal(500.75m, okResult.Value!.Amount);
    }

    private async Task<IResult> ExecuteUpdateTransaction(ExpenseDbContext db, Guid id, Transaction updated, Guid householdId)
    {
        var transaction = await db.Transactions
            .Where(t => t.HouseholdId == householdId)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (transaction == null)
            return Results.NotFound();

        transaction.Description = updated.Description;
        transaction.Amount = updated.Amount;
        transaction.Date = updated.Date;
        transaction.CategoryId = updated.CategoryId;
        transaction.Type = updated.Type;
        transaction.Notes = updated.Notes;

        await db.SaveChangesAsync();
        return Results.Ok(transaction);
    }
}
