using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Transactions.Delete
{
    public static class DeleteTransactionEndpoint
    {
        public static RouteHandlerBuilder MapDeleteTransactionEndpoint(this IEndpointRouteBuilder app)
        {
            return app.MapDelete("/api/transactions/{id}", async (ExpenseDbContext db, Guid id) =>
            {
                var transaction = await db.Transactions.FindAsync(id);
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
}
