using ExpenseControl.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Persistence;

/// <summary>
/// Seeds realistic data for the mock development user so the UI is populated out of the box.
/// This seeder is intentionally idempotent: it only runs when the mock household is absent.
/// </summary>
public static class DevSeedData
{
    public static readonly Guid MockHouseholdId = new("11111111-1111-1111-1111-111111111111");

    // Category IDs defined in SeedData.cs
    private static readonly Guid HousingCategoryId       = new("a48cde78-354e-4d5c-9159-cf28368fcaca");
    private static readonly Guid TransportationCategoryId = new("eb318b9d-aafc-421d-8041-64c6889ffc3c");
    private static readonly Guid FoodCategoryId           = new("c7e8d65f-f4b2-4162-ba57-04f52fb16d51");
    private static readonly Guid UtilitiesCategoryId      = new("ac800e65-9ae2-4274-8c8a-ae4658345c99");
    private static readonly Guid HealthcareCategoryId     = new("92974cbf-e03c-4f33-9e36-9f9f0a97523e");
    private static readonly Guid SalaryCategoryId         = new("3feb665e-56c5-4251-bda6-d665dbda65d3");
    private static readonly Guid FreelanceCategoryId      = new("51630c3e-35be-4b55-87a9-68ef640f772c");

    public static async Task SeedAsync(ExpenseDbContext db)
    {
        if (await db.Households.AnyAsync(h => h.Id == MockHouseholdId))
            return;

        var now = DateTime.UtcNow;

        db.Households.Add(new Household
        {
            Id = MockHouseholdId,
            Name = "Mock User's Household",
            CreatedAt = now
        });

        db.Transactions.AddRange(BuildTransactions(now));
        db.Budgets.AddRange(BuildBudgets(now));

        await db.SaveChangesAsync();
    }

    private static IEnumerable<Transaction> BuildTransactions(DateTime now)
    {
        // Generate 3 months of realistic transactions so balance/monthly charts are populated.
        for (int monthOffset = 2; monthOffset >= 0; monthOffset--)
        {
            var refDate = now.AddMonths(-monthOffset);
            int year = refDate.Year;
            int month = refDate.Month;

            // Income
            yield return MakeIncome("Monthly Salary",   4500m, year, month, 1,  SalaryCategoryId);
            yield return MakeIncome("Freelance Project", 800m, year, month, 12, FreelanceCategoryId);

            // Expenses
            yield return MakeExpense("Monthly Rent",       1500m, year, month, 1,  HousingCategoryId);
            yield return MakeExpense("Grocery Shopping",    250m, year, month, 5,  FoodCategoryId);
            yield return MakeExpense("Electricity Bill",    120m, year, month, 7,  UtilitiesCategoryId);
            yield return MakeExpense("Gas & Fuel",           80m, year, month, 10, TransportationCategoryId);
            yield return MakeExpense("Doctor Appointment",   60m, year, month, 15, HealthcareCategoryId);
        }
    }

    private static IEnumerable<Budget> BuildBudgets(DateTime now)
    {
        int month = now.Month;
        int year  = now.Year;

        yield return MakeBudget(HousingCategoryId,       1600m, month, year, now);
        yield return MakeBudget(FoodCategoryId,           400m, month, year, now);
        yield return MakeBudget(TransportationCategoryId, 150m, month, year, now);
        yield return MakeBudget(UtilitiesCategoryId,      200m, month, year, now);
        yield return MakeBudget(HealthcareCategoryId,     100m, month, year, now);
    }

    private static Expense MakeExpense(string description, decimal amount, int year, int month, int day, Guid categoryId)
        => new()
        {
            Id = Guid.NewGuid(),
            Description = description,
            Amount = amount,
            Date = new DateOnly(year, month, day),
            CategoryId = categoryId,
            HouseholdId = MockHouseholdId,
            Notes = string.Empty
        };

    private static Income MakeIncome(string description, decimal amount, int year, int month, int day, Guid categoryId)
        => new()
        {
            Id = Guid.NewGuid(),
            Description = description,
            Amount = amount,
            Date = new DateOnly(year, month, day),
            CategoryId = categoryId,
            HouseholdId = MockHouseholdId,
            Notes = string.Empty
        };

    private static Budget MakeBudget(Guid categoryId, decimal amount, int month, int year, DateTime now)
        => new()
        {
            Id = Guid.NewGuid(),
            CategoryId = categoryId,
            Amount = amount,
            Month = month,
            Year = year,
            HouseholdId = MockHouseholdId,
            CreatedAt = now,
            UpdatedAt = now
        };
}
