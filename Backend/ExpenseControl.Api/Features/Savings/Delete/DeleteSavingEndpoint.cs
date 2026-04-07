using System.Security.Claims;
using ExpenseControl.Api.Features.Auth;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Savings.Delete;

public static class DeleteSavingEndpoint
{
    public static void MapDeleteSavingEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/savings/{id:guid}", async (ExpenseDbContext db, ClaimsPrincipal user, Guid id) =>
        {
            var householdId = user.GetHouseholdId();

            var saving = await db.Savings.FirstOrDefaultAsync(s =>
                s.Id == id && s.HouseholdId == householdId);

            if (saving == null)
                return Results.NotFound("Saving not found");

            db.Savings.Remove(saving);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}
