using System.Security.Claims;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Auth;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Savings.Create;

public static class CreateSavingEndpoint
{
    public static void MapCreateSavingEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/savings", async (ExpenseDbContext db, ClaimsPrincipal user, CreateSavingRequest request) =>
        {
            var householdId = user.GetHouseholdId();

            var household = await db.Households.FindAsync(householdId);
            if (household == null)
                return Results.NotFound("Household not found");

            var saving = new Saving
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                CurrentAmount = request.CurrentAmount,
                TargetAmount = request.TargetAmount,
                HouseholdId = householdId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Savings.Add(saving);
            await db.SaveChangesAsync();
            return Results.Ok(saving.ToResponse());
        });
    }
}
