using System.Security.Claims;
using ExpenseControl.Api.Features.Auth;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Savings.Update;

public static class UpdateSavingEndpoint
{
    public static void MapUpdateSavingEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/savings/{id:guid}", async (ExpenseDbContext db, ClaimsPrincipal user, Guid id, UpdateSavingRequest request) =>
        {
            var householdId = user.GetHouseholdId();

            var saving = await db.Savings.FirstOrDefaultAsync(s =>
                s.Id == id && s.HouseholdId == householdId);

            if (saving == null)
                return Results.NotFound("Saving not found");

            saving.Name = request.Name;
            saving.Description = request.Description;
            saving.CurrentAmount = request.CurrentAmount;
            saving.TargetAmount = request.TargetAmount;
            saving.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
            return Results.Ok(saving.ToResponse());
        });
    }
}
