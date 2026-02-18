using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Persistence;
using ExpenseControl.Api.Tests.TestHelpers;

namespace ExpenseControl.Api.Tests.Persistence;

public class DevSeedDataTests
{
    [Fact]
    public async Task SeedAsync_WhenHouseholdAbsent_CreatesHousehold()
    {
        // Arrange
        await using var db = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());

        // Act
        await DevSeedData.SeedAsync(db);

        // Assert
        var household = await db.Households.FindAsync(DevSeedData.MockHouseholdId);
        Assert.NotNull(household);
        Assert.Equal("Mock User's Household", household.Name);
    }

    [Fact]
    public async Task SeedAsync_WhenHouseholdAbsent_CreatesTransactions()
    {
        // Arrange
        await using var db = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());

        // Act
        await DevSeedData.SeedAsync(db);

        // Assert
        var transactions = db.Transactions
            .Where(t => t.HouseholdId == DevSeedData.MockHouseholdId)
            .ToList();

        Assert.NotEmpty(transactions);
    }

    [Fact]
    public async Task SeedAsync_WhenHouseholdAbsent_CreatesBothExpensesAndIncomes()
    {
        // Arrange
        await using var db = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());

        // Act
        await DevSeedData.SeedAsync(db);

        // Assert
        var transactions = db.Transactions
            .Where(t => t.HouseholdId == DevSeedData.MockHouseholdId)
            .ToList();

        Assert.Contains(transactions, t => t.Type == TransactionType.Expense);
        Assert.Contains(transactions, t => t.Type == TransactionType.Income);
    }

    [Fact]
    public async Task SeedAsync_WhenHouseholdAbsent_CreatesBudgets()
    {
        // Arrange
        await using var db = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());

        // Act
        await DevSeedData.SeedAsync(db);

        // Assert
        var budgets = db.Budgets
            .Where(b => b.HouseholdId == DevSeedData.MockHouseholdId)
            .ToList();

        Assert.NotEmpty(budgets);
    }

    [Fact]
    public async Task SeedAsync_WhenHouseholdAbsent_CreatesBudgetsForCurrentMonth()
    {
        // Arrange
        await using var db = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());
        var now = DateTime.UtcNow;

        // Act
        await DevSeedData.SeedAsync(db);

        // Assert
        var budgets = db.Budgets
            .Where(b => b.HouseholdId == DevSeedData.MockHouseholdId)
            .ToList();

        Assert.All(budgets, b =>
        {
            Assert.Equal(now.Month, b.Month);
            Assert.Equal(now.Year, b.Year);
        });
    }

    [Fact]
    public async Task SeedAsync_IsIdempotent_WhenCalledTwice()
    {
        // Arrange
        await using var db = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());

        // Act
        await DevSeedData.SeedAsync(db);
        await DevSeedData.SeedAsync(db);

        // Assert — household should exist exactly once
        var householdCount = db.Households
            .Count(h => h.Id == DevSeedData.MockHouseholdId);

        Assert.Equal(1, householdCount);
    }

    [Fact]
    public async Task SeedAsync_IsIdempotent_DoesNotDuplicateTransactions()
    {
        // Arrange
        await using var db = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());

        await DevSeedData.SeedAsync(db);
        var countAfterFirst = db.Transactions
            .Count(t => t.HouseholdId == DevSeedData.MockHouseholdId);

        // Act
        await DevSeedData.SeedAsync(db);

        // Assert
        var countAfterSecond = db.Transactions
            .Count(t => t.HouseholdId == DevSeedData.MockHouseholdId);

        Assert.Equal(countAfterFirst, countAfterSecond);
    }

    [Fact]
    public async Task SeedAsync_WhenHouseholdAlreadyExists_DoesNothing()
    {
        // Arrange
        await using var db = DbContextHelper.CreateInMemoryDbContext(Guid.NewGuid().ToString());

        var preExistingHousehold = new Household
        {
            Id = DevSeedData.MockHouseholdId,
            Name = "Already Exists",
            CreatedAt = DateTime.UtcNow
        };
        db.Households.Add(preExistingHousehold);
        await db.SaveChangesAsync();

        // Act
        await DevSeedData.SeedAsync(db);

        // Assert — name should remain unchanged; no transactions seeded
        var household = await db.Households.FindAsync(DevSeedData.MockHouseholdId);
        Assert.Equal("Already Exists", household!.Name);

        var transactionCount = db.Transactions
            .Count(t => t.HouseholdId == DevSeedData.MockHouseholdId);
        Assert.Equal(0, transactionCount);
    }
}
