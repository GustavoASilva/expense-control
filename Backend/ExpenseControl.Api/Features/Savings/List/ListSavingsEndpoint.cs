using System.Security.Claims;
using ExpenseControl.Api.Features.Auth;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Savings.List;

public static class ListSavingsEndpoint
{
    public static void MapListSavingsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/savings", async (ExpenseDbContext db, ClaimsPrincipal user) =>
        {
            var householdId = user.GetHouseholdId();
            var savings = await db.Savings
                .Where(s => s.HouseholdId == householdId)
                .OrderBy(s => s.Name)
                .ToListAsync();
            return Results.Ok(savings.ToResponse());
        });
    }
}
