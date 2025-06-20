using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Balance
{
    public static class GetBalanceByCategoryEndpoint
    {
        public static RouteHandlerBuilder MapGetBalanceByCategoryEndpoint(this IEndpointRouteBuilder app)
        {
            return app.MapGet("/api/balance/by-category", async (ExpenseDbContext db, DateTime? startDate, DateTime? endDate) =>
            {
                // Normalize date filters to UTC
                if (startDate.HasValue && startDate.Value.Kind != DateTimeKind.Utc)
                    startDate = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
                if (endDate.HasValue && endDate.Value.Kind != DateTimeKind.Utc)
                    endDate = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);

                // Handle date range
                var periodStart = startDate ?? DateTime.UtcNow.Date.AddMonths(-1);
                var periodEnd = (endDate ?? DateTime.UtcNow.Date).Date.AddDays(1).AddTicks(-1);

                var query = db.Transactions
                    .Include(t => t.Category)
                    .Where(t => t.Date >= periodStart && t.Date <= periodEnd);

                var categories = await db.Categories.ToListAsync();
                var transactions = await query.ToListAsync();

                var result = categories.SelectMany(c => new[]
                {
                    new
                    {
                        CategoryName = c.Name,
                        TransactionType = TransactionType.Income,
                        Total = transactions
                            .Where(t => t.CategoryId == c.Id && t.Type == TransactionType.Income)
                            .Sum(t => t.Amount),
                        Count = transactions
                            .Count(t => t.CategoryId == c.Id && t.Type == TransactionType.Income)
                    },
                    new
                    {
                        CategoryName = c.Name,
                        TransactionType = TransactionType.Expense,
                        Total = transactions
                            .Where(t => t.CategoryId == c.Id && t.Type == TransactionType.Expense)
                            .Sum(t => t.Amount),
                        Count = transactions
                            .Count(t => t.CategoryId == c.Id && t.Type == TransactionType.Expense)
                    }
                })
                .Where(r => r.Count > 0)
                .OrderByDescending(r => r.Total);

                return Results.Ok(new
                {
                    Categories = result,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    HasTransactions = transactions.Any()
                });
            })
            .WithName("GetBalanceByCategory")
            .WithOpenApi();
        }
    }
}
