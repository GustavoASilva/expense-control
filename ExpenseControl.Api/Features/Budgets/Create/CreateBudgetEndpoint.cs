using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Budgets.Create
{
    public static class CreateBudgetEndpoint
    {
        public static void MapCreateBudgetEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/budgets", async (ExpenseDbContext db, Budget budget) =>
            {
                if (budget.HouseholdId.HasValue)
                {
                    var household = await db.Households.FindAsync(budget.HouseholdId.Value);
                    if (household == null)
                        return Results.NotFound("Household not found");
                }

                var existingQuery = db.Budgets.Where(b => b.CategoryId == budget.CategoryId && b.Month == budget.Month && b.Year == budget.Year);

                if (budget.HouseholdId.HasValue)
                    existingQuery = existingQuery.Where(b => b.HouseholdId == budget.HouseholdId.Value);
                else
                    existingQuery = existingQuery.Where(b => b.HouseholdId == null);

                var existing = await existingQuery.FirstOrDefaultAsync();
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
            });
        }
    }
}
