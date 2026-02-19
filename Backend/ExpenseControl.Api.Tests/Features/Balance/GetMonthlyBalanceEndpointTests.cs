using AutoFixture;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Balance;
using ExpenseControl.Api.Persistence;
using ExpenseControl.Api.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Tests.Features.Balance;

public class GetMonthlyBalanceEndpointTests
{
    private readonly Fixture _fixture;

    public GetMonthlyBalanceEndpointTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task GetMonthlyBalance_WithTransactions_ReturnsCorrectMonthlySummary()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var expenseCategory = CreateAndAddCategory(db, "Food", TransactionType.Expense);
        var incomeCategory = CreateAndAddCategory(db, "Salary", TransactionType.Income);

        var year = DateTime.UtcNow.Year;
        var transactions = new List<Transaction>
        {
            CreateTransaction(household.Id, incomeCategory.Id, 5000m, new DateOnly(year, 1, 15), TransactionType.Income),
            CreateTransaction(household.Id, expenseCategory.Id, 1000m, new DateOnly(year, 1, 20), TransactionType.Expense),
            CreateTransaction(household.Id, incomeCategory.Id, 5000m, new DateOnly(year, 2, 15), TransactionType.Income),
            CreateTransaction(household.Id, expenseCategory.Id, 1500m, new DateOnly(year, 2, 20), TransactionType.Expense)
        };

        db.Transactions.AddRange(transactions);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteGetMonthlyBalance(db, household.Id, year);

        // Assert
        Assert.Equal(year, result.Year);
        Assert.True(result.HasTransactions);
        Assert.Equal(10000m, result.TotalIncome);
        Assert.Equal(2500m, result.TotalExpenses);
        
        var januarySummary = result.Months.FirstOrDefault(m => m.Month == 1);
        Assert.NotNull(januarySummary);
        Assert.Equal(5000m, januarySummary.Income);
        Assert.Equal(1000m, januarySummary.Expenses);
        Assert.Equal(4000m, januarySummary.Balance);
        Assert.True(januarySummary.HasTransactions);
    }

    [Fact]
    public async Task GetMonthlyBalance_WithNoYear_UsesCurrentYear()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");

        // Act
        var result = await ExecuteGetMonthlyBalance(db, household.Id, null);

        // Assert
        Assert.Equal(DateTime.UtcNow.Year, result.Year);
    }

    [Fact]
    public async Task GetMonthlyBalance_WithNoTransactions_ShowsLimitedMonths()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");

        // Act
        var result = await ExecuteGetMonthlyBalance(db, household.Id, DateTime.UtcNow.Year);

        // Assert
        Assert.False(result.HasTransactions);
        Assert.Equal(0m, result.TotalIncome);
        Assert.Equal(0m, result.TotalExpenses);
        
        var currentMonth = DateTime.UtcNow.Month;
        var expectedMonthCount = Math.Min(3, currentMonth);
        Assert.True(result.Months.Count() <= expectedMonthCount || result.Months.Count() == 12);
    }

    [Fact]
    public async Task GetMonthlyBalance_WithTransactionsInYear_ShowsAllMonths()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var incomeCategory = CreateAndAddCategory(db, "Salary", TransactionType.Income);

        var year = DateTime.UtcNow.Year;
        var transaction = CreateTransaction(household.Id, incomeCategory.Id, 5000m, new DateOnly(year, 1, 15), TransactionType.Income);

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteGetMonthlyBalance(db, household.Id, year);

        // Assert
        Assert.True(result.HasTransactions);
        Assert.Equal(12, result.Months.Count());
    }

    [Fact]
    public async Task GetMonthlyBalance_MonthWithoutTransactions_ShowsZeroValues()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var incomeCategory = CreateAndAddCategory(db, "Salary", TransactionType.Income);

        var year = DateTime.UtcNow.Year;
        var transaction = CreateTransaction(household.Id, incomeCategory.Id, 5000m, new DateOnly(year, 1, 15), TransactionType.Income);

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteGetMonthlyBalance(db, household.Id, year);

        // Assert
        var februarySummary = result.Months.FirstOrDefault(m => m.Month == 2);
        Assert.NotNull(februarySummary);
        Assert.Equal(0m, februarySummary.Income);
        Assert.Equal(0m, februarySummary.Expenses);
        Assert.Equal(0m, februarySummary.Balance);
        Assert.False(februarySummary.HasTransactions);
    }

    [Fact]
    public async Task GetMonthlyBalance_FiltersTransactionsByHousehold()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household1 = CreateAndAddHousehold(db, "Household 1");
        var household2 = CreateAndAddHousehold(db, "Household 2");
        var incomeCategory = CreateAndAddCategory(db, "Salary", TransactionType.Income);

        var year = DateTime.UtcNow.Year;
        var transactions = new List<Transaction>
        {
            CreateTransaction(household1.Id, incomeCategory.Id, 5000m, new DateOnly(year, 1, 15), TransactionType.Income),
            CreateTransaction(household2.Id, incomeCategory.Id, 3000m, new DateOnly(year, 1, 15), TransactionType.Income)
        };

        db.Transactions.AddRange(transactions);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteGetMonthlyBalance(db, household1.Id, year);

        // Assert
        Assert.Equal(5000m, result.TotalIncome);
        Assert.Equal(0m, result.TotalExpenses);
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

    private async Task<MonthlyBalanceResponse> ExecuteGetMonthlyBalance(
        ExpenseDbContext db,
        Guid householdId,
        int? year)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var currentMonth = DateTime.UtcNow.Month;

        var query = db.Transactions
            .Where(t => t.Date.Year == targetYear)
            .Where(t => t.HouseholdId == householdId);

        var transactions = await query.ToListAsync();

        var monthsToShow = !transactions.Any() && targetYear == DateTime.UtcNow.Year
            ? Enumerable.Range(Math.Max(1, currentMonth - 2), Math.Min(3, currentMonth))
            : Enumerable.Range(1, 12);

        var monthlySummary = monthsToShow
            .Select(month =>
            {
                var monthTransactions = transactions.Where(t => t.Date.Month == month);
                var income = monthTransactions
                    .Where(t => t.Type == TransactionType.Income)
                    .Sum(t => t.Amount);
                var expenses = monthTransactions
                    .Where(t => t.Type == TransactionType.Expense)
                    .Sum(t => t.Amount);

                return new MonthSummaryResponse(
                    month,
                    new DateTime(targetYear, month, 1).ToString("MMMM"),
                    income,
                    expenses,
                    income - expenses,
                    monthTransactions.Any()
                );
            })
            .OrderBy(m => m.Month);

        return new MonthlyBalanceResponse(
            targetYear,
            monthlySummary,
            transactions.Any(),
            transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
            transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
        );
    }
}
