using ExpenseControl.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Transactions.List
{
    public record ListTransactionsQuery(
        TransactionType? Type = null,
        Guid? CategoryId = null,
        DateTime? StartDate = null,
        DateTime? EndDate = null,
        int? Limit = null,
        int? Offset = null
    );

    public static class ListTransactionsEndpoint
    {
        public static void MapListTransactionsEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/transactions", async (
                ExpenseControl.Api.Persistence.ExpenseDbContext db,
                TransactionType? type,
                Guid? categoryId,
                DateTime? startDate,
                DateTime? endDate,
                int? limit,
                int? offset) =>
            {
                // Normalize date filters to UTC
                if (startDate.HasValue && startDate.Value.Kind != DateTimeKind.Utc)
                    startDate = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
                if (endDate.HasValue && endDate.Value.Kind != DateTimeKind.Utc)
                    endDate = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);

                var query = db.Transactions
                    .Include(t => t.Category)
                    .AsQueryable();

                if (type.HasValue)
                    query = query.Where(t => t.Type == type.Value);

                if (categoryId.HasValue)
                    query = query.Where(t => t.CategoryId == categoryId.Value);

                if (startDate.HasValue)
                    query = query.Where(t => t.Date >= startDate.Value);

                if (endDate.HasValue)
                {
                    var inclusiveEnd = endDate.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(t => t.Date <= inclusiveEnd);
                }

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
