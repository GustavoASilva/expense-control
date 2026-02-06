using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Budgets.List
{
    public static class ListBudgetsEndpoint
    {
        public static void MapListBudgetsEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budgets", async (ExpenseDbContext db, int? year, int? month, Guid? categoryId, Guid? householdId) =>
            {
                var query = db.Budgets.Include(b => b.Category).AsQueryable();
                if (householdId.HasValue) query = query.Where(b => b.HouseholdId == householdId.Value);
                if (year.HasValue) query = query.Where(b => b.Year == year);
                if (month.HasValue) query = query.Where(b => b.Month == month);
                if (categoryId.HasValue) query = query.Where(b => b.CategoryId == categoryId);
                var budgets = await query.ToListAsync();
                return Results.Ok(budgets);
            });
        }
    }
}
