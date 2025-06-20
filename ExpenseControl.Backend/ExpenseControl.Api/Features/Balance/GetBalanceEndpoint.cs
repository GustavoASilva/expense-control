using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Balance
{
    public static class GetBalanceEndpoint
    {
        public static RouteHandlerBuilder MapGetBalanceEndpoint(this IEndpointRouteBuilder app)
        {
            return app.MapGet("/api/balance", async (ExpenseDbContext db, DateTime? startDate, DateTime? endDate) =>
            {
                // Normalize date filters to UTC
                if (startDate.HasValue && startDate.Value.Kind != DateTimeKind.Utc)
                    startDate = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
                if (endDate.HasValue && endDate.Value.Kind != DateTimeKind.Utc)
                    endDate = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);

                var query = db.Transactions.AsQueryable();

                // Handle date range
                var periodStart = startDate ?? DateTime.UtcNow.Date.AddMonths(-1);
                var periodEnd = (endDate ?? DateTime.UtcNow.Date).Date.AddDays(1).AddTicks(-1);

                query = query.Where(t => t.Date >= periodStart && t.Date <= periodEnd);

                var result = await query.GroupBy(t => t.Type)
                    .Select(g => new { Type = g.Key, Total = g.Sum(t => t.Amount) })
                    .ToListAsync();

                var income = result.FirstOrDefault(r => r.Type == TransactionType.Income)?.Total ?? 0;
                var expenses = result.FirstOrDefault(r => r.Type == TransactionType.Expense)?.Total ?? 0;

                return Results.Ok(new
                {
                    Income = income,
                    Expenses = expenses,
                    Balance = income - expenses,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    HasTransactions = result.Any()
                });
            })
            .WithName("GetBalance")
            .WithOpenApi();
        }
    }
}
