using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Budgets.Usage
{
    public static class GetBudgetUsageEndpoint
    {
        public static void MapGetBudgetUsageEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budgets/usage", async (ExpenseDbContext db, Guid categoryId, int year, int month) =>
            {
                var budget = await db.Budgets.FirstOrDefaultAsync(b => b.CategoryId == categoryId && b.Year == year && b.Month == month);
                if (budget == null) return Results.NotFound();
                var usage = await db.Transactions
                    .Where(t => t.CategoryId == categoryId && t.Date.Year == year && t.Date.Month == month && t.Type == TransactionType.Expense)
                    .SumAsync(t => t.Amount);
                var percent = budget.Amount > 0 ? (usage / budget.Amount) * 100m : 0m;
                return Results.Ok(new { budget.Amount, usage, percent });
            });
        }
    }
}
