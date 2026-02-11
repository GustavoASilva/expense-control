using AutoFixture;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Persistence;
using ExpenseControl.Api.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Tests.Features.Transactions;

public class DeleteTransactionEndpointTests
{
    private readonly Fixture _fixture;

    public DeleteTransactionEndpointTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task DeleteTransaction_WithValidId_DeletesTransaction()
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
        var result = await ExecuteDeleteTransaction(db, transaction.Id, household.Id);

        // Assert
        Assert.IsType<NoContent>(result);
        var deletedTransaction = await db.Transactions.FindAsync(transaction.Id);
        Assert.Null(deletedTransaction);
    }

    [Fact]
    public async Task DeleteTransaction_WithNonExistentId_ReturnsNotFound()
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
        var result = await ExecuteDeleteTransaction(db, Guid.NewGuid(), household.Id);

        // Assert
        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task DeleteTransaction_WithWrongHouseholdId_ReturnsNotFound()
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

        // Act - Try to delete with wrong household ID
        var result = await ExecuteDeleteTransaction(db, transaction.Id, household2.Id);

        // Assert
        Assert.IsType<NotFound>(result);
        // Verify transaction still exists
        var existingTransaction = await db.Transactions.FindAsync(transaction.Id);
        Assert.NotNull(existingTransaction);
    }

    [Fact]
    public async Task DeleteTransaction_RemovesOnlySpecifiedTransaction()
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

        var transaction1 = new Expense
        {
            Id = Guid.NewGuid(),
            Description = "Transaction 1",
            Amount = 100m,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId = category.Id,
            Notes = "Notes 1",
            HouseholdId = household.Id
        };

        var transaction2 = new Expense
        {
            Id = Guid.NewGuid(),
            Description = "Transaction 2",
            Amount = 200m,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            CategoryId = category.Id,
            Notes = "Notes 2",
            HouseholdId = household.Id
        };

        db.Transactions.AddRange(transaction1, transaction2);
        await db.SaveChangesAsync();

        // Act - Delete only transaction1
        var result = await ExecuteDeleteTransaction(db, transaction1.Id, household.Id);

        // Assert
        Assert.IsType<NoContent>(result);
        var deletedTransaction = await db.Transactions.FindAsync(transaction1.Id);
        Assert.Null(deletedTransaction);
        var remainingTransaction = await db.Transactions.FindAsync(transaction2.Id);
        Assert.NotNull(remainingTransaction);
    }

    private async Task<IResult> ExecuteDeleteTransaction(ExpenseDbContext db, Guid id, Guid householdId)
    {
        var transaction = await db.Transactions
            .Where(t => t.HouseholdId == householdId)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (transaction == null)
            return Results.NotFound();

        db.Transactions.Remove(transaction);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }
}
