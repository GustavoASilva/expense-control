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
        app.MapPost("/api/budgets", async (ExpenseDbContext db, ClaimsPrincipal user, CreateBudgetRequest request) =>
        {
            var householdId = user.GetHouseholdId();

            var household = await db.Households.FindAsync(householdId);
            if (household == null)
                return Results.NotFound("Household not found");

            var existing = await db.Budgets.FirstOrDefaultAsync(b =>
                b.CategoryId == request.CategoryId &&
                b.Month == request.Month &&
                b.Year == request.Year &&
                b.HouseholdId == householdId);

            if (existing != null)
            {
                existing.Amount = request.Amount;
                existing.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
                return Results.Ok(existing.ToResponse());
            }

            var budget = new Budget
            {
                Id = Guid.NewGuid(),
                CategoryId = request.CategoryId,
                Amount = request.Amount,
                Month = request.Month,
                Year = request.Year,
                HouseholdId = householdId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Budgets.Add(budget);
            await db.SaveChangesAsync();
            return Results.Ok(budget.ToResponse());
        });
    }
}
