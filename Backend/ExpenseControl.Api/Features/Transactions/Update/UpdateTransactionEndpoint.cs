using System.Security.Claims;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Auth;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Transactions.Update;

public static class UpdateTransactionEndpoint
{
    public static RouteHandlerBuilder MapUpdateTransactionEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapPatch("/api/transactions/{id}", async (ExpenseDbContext db, ClaimsPrincipal user, Guid id, Transaction updated) =>
        {
            var householdId = user.GetHouseholdId();
            var transaction = await db.Transactions
                .Where(t => t.HouseholdId == householdId)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (transaction == null)
                return Results.NotFound();

            transaction.Description = updated.Description;
            transaction.Amount = updated.Amount;
            transaction.Date = updated.Date;
            transaction.CategoryId = updated.CategoryId;
            transaction.Type = updated.Type;
            transaction.Notes = updated.Notes;

            await db.SaveChangesAsync();
            return Results.Ok(transaction);
        })
        .WithName("UpdateTransaction")
        .WithOpenApi();
    }
}
