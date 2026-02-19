using System.Security.Claims;
using ExpenseControl.Api.Features.Auth;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Transactions.Update;

public static class UpdateTransactionEndpoint
{
    public static RouteHandlerBuilder MapUpdateTransactionEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapPatch("/api/transactions/{id}", async (ExpenseDbContext db, ClaimsPrincipal user, Guid id, UpdateTransactionRequest request) =>
        {
            var householdId = user.GetHouseholdId();
            var transaction = await db.Transactions
                .Where(t => t.HouseholdId == householdId)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (transaction == null)
                return Results.NotFound();

            transaction.Description = request.Description;
            transaction.Amount = request.Amount;
            transaction.Date = request.Date;
            transaction.CategoryId = request.CategoryId;
            transaction.Type = request.Type;
            transaction.Notes = request.Notes;

            await db.SaveChangesAsync();
            return Results.Ok(transaction.ToResponse());
        })
        .WithName("UpdateTransaction")
        .WithOpenApi();
    }
}
