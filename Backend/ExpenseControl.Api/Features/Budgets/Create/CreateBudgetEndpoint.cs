using System.Security.Claims;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Auth;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Budgets.Create;

public static class CreateBudgetEndpoint
{
    public static void MapCreateBudgetEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/budgets", async (ExpenseDbContext db, ClaimsPrincipal user, Budget budget) =>
        {
            var householdId = user.GetHouseholdId();
            budget.HouseholdId = householdId;

            var household = await db.Households.FindAsync(householdId);
            if (household == null)
                return Results.NotFound("Household not found");

            var existing = await db.Budgets.FirstOrDefaultAsync(b =>
                b.CategoryId == budget.CategoryId &&
                b.Month == budget.Month &&
                b.Year == budget.Year &&
                b.HouseholdId == budget.HouseholdId);

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
