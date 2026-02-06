using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Transactions.Get
{
    public static class GetTransactionEndpoint
    {
        public static RouteHandlerBuilder MapGetTransactionEndpoint(this IEndpointRouteBuilder app)
        {
            return app.MapGet("/api/transactions/{id}", async (ExpenseDbContext db, Guid id, Guid householdId) =>
            {
                var transaction = await db.Transactions
                    .Include(t => t.Category)
                    .Where(t => t.HouseholdId == householdId)
                    .FirstOrDefaultAsync(t => t.Id == id);

                return transaction is null ? Results.NotFound() : Results.Ok(transaction);
            })
            .WithName("GetTransaction")
            .WithOpenApi();
        }
    }
}
