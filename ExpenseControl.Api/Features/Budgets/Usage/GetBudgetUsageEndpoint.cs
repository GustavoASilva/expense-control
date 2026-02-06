using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Budgets.Usage
{
    public static class GetBudgetUsageEndpoint
    {
        public static void MapGetBudgetUsageEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budgets/usage", async (ExpenseDbContext db, Guid categoryId, int year, int month, Guid? householdId) =>
            {
                var budgetQuery = db.Budgets.Where(b => b.CategoryId == categoryId && b.Year == year && b.Month == month);

                if (householdId.HasValue)
                    budgetQuery = budgetQuery.Where(b => b.HouseholdId == householdId.Value);

                var budget = await budgetQuery.FirstOrDefaultAsync();
                if (budget == null) return Results.NotFound();

                var transactionQuery = db.Transactions
                    .Where(t => t.CategoryId == categoryId && t.Date.Year == year && t.Date.Month == month && t.Type == TransactionType.Expense);

                if (householdId.HasValue)
                    transactionQuery = transactionQuery.Where(t => t.HouseholdId == householdId.Value);

                var usage = await transactionQuery.SumAsync(t => t.Amount);
                var percent = budget.Amount > 0 ? (usage / budget.Amount) * 100m : 0m;
                return Results.Ok(new { budget.Amount, usage, percent });
            });
        }
    }
}
