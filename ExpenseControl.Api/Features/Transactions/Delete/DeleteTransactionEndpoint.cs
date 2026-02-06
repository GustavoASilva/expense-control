using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Transactions.Delete
{
    public static class DeleteTransactionEndpoint
    {
        public static RouteHandlerBuilder MapDeleteTransactionEndpoint(this IEndpointRouteBuilder app)
        {
            return app.MapDelete("/api/transactions/{id}", async (ExpenseDbContext db, Guid id, Guid? householdId) =>
            {
                var query = db.Transactions.AsQueryable();

                if (householdId.HasValue)
                    query = query.Where(t => t.HouseholdId == householdId.Value);

                var transaction = await query.FirstOrDefaultAsync(t => t.Id == id);
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
