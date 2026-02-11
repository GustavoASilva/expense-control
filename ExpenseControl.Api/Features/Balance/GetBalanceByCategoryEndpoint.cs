using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Balance;

public static class GetBalanceByCategoryEndpoint
{
    public static RouteHandlerBuilder MapGetBalanceByCategoryEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/api/balance/by-category", async (ExpenseDbContext db, Guid householdId, DateOnly? startDate, DateOnly? endDate) =>
        {
            var periodStart = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(-1));
            var periodEnd = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);

            var transactionQuery = db.Transactions
                .Include(t => t.Category)
                .Where(t => t.Date >= periodStart && t.Date <= periodEnd)
                .Where(t => t.HouseholdId == householdId);

            var categories = await db.Categories.ToListAsync();
            var transactions = await transactionQuery.ToListAsync();

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
        .RequireAuthorization()
        .WithName("GetBalanceByCategory")
        .WithOpenApi();
    }
}
