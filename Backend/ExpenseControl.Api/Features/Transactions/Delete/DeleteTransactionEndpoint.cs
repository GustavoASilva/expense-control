using System.Security.Claims;
using ExpenseControl.Api.Features.Auth;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Transactions.Delete;

public static class DeleteTransactionEndpoint
{
    public static RouteHandlerBuilder MapDeleteTransactionEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapDelete("/api/transactions/{id}", async (ExpenseDbContext db, ClaimsPrincipal user, Guid id) =>
        {
            var householdId = user.GetHouseholdId();
            var transaction = await db.Transactions
                .Where(t => t.HouseholdId == householdId)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (transaction == null)
                return Results.NotFound();

            db.Transactions.Remove(transaction);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeleteTransaction")
        .WithOpenApi();
    }
}
