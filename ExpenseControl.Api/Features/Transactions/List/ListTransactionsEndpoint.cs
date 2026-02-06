using ExpenseControl.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Transactions.List
{
    public record ListTransactionsQuery(
        TransactionType? Type = null,
        Guid? CategoryId = null,
        DateOnly? StartDate = null,
        DateOnly? EndDate = null,
        int? Limit = null,
        int? Offset = null,
        Guid? HouseholdId = null
    );

    public static class ListTransactionsEndpoint
    {
        public static void MapListTransactionsEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/transactions", async (
                ExpenseControl.Api.Persistence.ExpenseDbContext db,
                TransactionType? type,
                Guid? categoryId,
                DateOnly? startDate,
                DateOnly? endDate,
                int? limit,
                int? offset,
                Guid? householdId) =>
            {
                var query = db.Transactions
                    .Include(t => t.Category)
                    .AsQueryable();

                if (householdId.HasValue)
                    query = query.Where(t => t.HouseholdId == householdId.Value);

                if (type.HasValue)
                    query = query.Where(t => t.Type == type.Value);

                if (categoryId.HasValue)
                    query = query.Where(t => t.CategoryId == categoryId.Value);

                if (startDate.HasValue)
                    query = query.Where(t => t.Date >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(t => t.Date <= endDate.Value);

                query = query.OrderByDescending(t => t.Date);

                if (offset.HasValue)
                    query = query.Skip(offset.Value);

                if (limit.HasValue)
                    query = query.Take(limit.Value);

                var transactions = await query.ToListAsync();
                return Results.Ok(transactions);
            })
            .WithName("ListTransactions")
            .WithOpenApi();
        }
    }
}
