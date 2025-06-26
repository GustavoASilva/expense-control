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
                var existing = await db.Budgets.FirstOrDefaultAsync(b => b.CategoryId == budget.CategoryId && b.Month == budget.Month && b.Year == budget.Year);
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
