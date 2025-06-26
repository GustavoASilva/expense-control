using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Budgets
{
    public static class BudgetEndpoints
    {
        public static void MapBudgetEndpoints(this IEndpointRouteBuilder app)
        {
            // Create or update a budget for a category/month/year
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

            // Get all budgets (optionally filter by year/month/category)
            app.MapGet("/api/budgets", async (ExpenseDbContext db, int? year, int? month, Guid? categoryId) =>
            {
                var query = db.Budgets.Include(b => b.Category).AsQueryable();
                if (year.HasValue) query = query.Where(b => b.Year == year);
                if (month.HasValue) query = query.Where(b => b.Month == month);
                if (categoryId.HasValue) query = query.Where(b => b.CategoryId == categoryId);
                var budgets = await query.ToListAsync();
                return Results.Ok(budgets);
            });

            // Get budget usage for a category/month/year
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
