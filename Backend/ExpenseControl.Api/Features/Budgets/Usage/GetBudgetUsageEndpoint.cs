using System.Security.Claims;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Auth;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Budgets.Usage;

public static class GetBudgetUsageEndpoint
{
    public static void MapGetBudgetUsageEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/budgets/usage", async (ExpenseDbContext db, ClaimsPrincipal user, Guid categoryId, int year, int month) =>
        {
            var householdId = user.GetHouseholdId();
            var budget = await db.Budgets.FirstOrDefaultAsync(b =>
                b.CategoryId == categoryId &&
                b.Year == year &&
                b.Month == month &&
                b.HouseholdId == householdId);
            if (budget == null) return Results.NotFound();

            // Project to only Amount before loading into memory.
            // Amount is stored as encrypted text and cannot be aggregated directly in the database.
            var amounts = await db.Transactions
                .Where(t => t.CategoryId == categoryId &&
                    t.Date.Year == year &&
                    t.Date.Month == month &&
                    t.Type == TransactionType.Expense &&
                    t.HouseholdId == householdId)
                .Select(t => t.Amount)
                .ToListAsync();
            var usage = amounts.Sum();
            var percent = budget.Amount > 0 ? (usage / budget.Amount) * 100m : 0m;
            return Results.Ok(new BudgetUsageResponse(budget.Amount, usage, percent));
        });
    }
}
