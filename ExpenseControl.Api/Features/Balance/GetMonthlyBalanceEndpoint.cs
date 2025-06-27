using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Balance
{
    public static class GetMonthlyBalanceEndpoint
    {
        public static RouteHandlerBuilder MapGetMonthlyBalanceEndpoint(this IEndpointRouteBuilder app)
        {
            return app.MapGet("/api/balance/monthly", async (ExpenseDbContext db, int? year) =>
            {
                // No date filter from client, but ensure all transaction dates are UTC
                var targetYear = year ?? DateTime.UtcNow.Year;
                var currentMonth = DateTime.UtcNow.Month;

                var query = db.Transactions.Where(t => t.Date.Year == targetYear);
                var transactions = await query.ToListAsync();

                // If no transactions and requesting current year, show last 3 months
                // If no transactions and requesting past year, show all months
                // If has transactions, show all months
                var monthsToShow = !transactions.Any() && targetYear == DateTime.UtcNow.Year
                    ? Enumerable.Range(Math.Max(1, currentMonth - 2), Math.Min(3, currentMonth))
                    : Enumerable.Range(1, 12);

                var monthlySummary = monthsToShow
                    .Select(month =>
                    {
                        var monthTransactions = transactions.Where(t => t.Date.Month == month);
                        var income = monthTransactions
                            .Where(t => t.Type == TransactionType.Income)
                            .Sum(t => t.Amount);
                        var expenses = monthTransactions
                            .Where(t => t.Type == TransactionType.Expense)
                            .Sum(t => t.Amount);

                        return new
                        {
                            Month = month,
                            MonthName = new DateTime(targetYear, month, 1).ToString("MMMM"),
                            Income = income,
                            Expenses = expenses,
                            Balance = income - expenses,
                            HasTransactions = monthTransactions.Any()
                        };
                    })
                    .OrderBy(m => m.Month);

                return Results.Ok(new
                {
                    Year = targetYear,
                    Months = monthlySummary,
                    HasTransactions = transactions.Any(),
                    TotalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                    TotalExpenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
                });
            })
            .WithName("GetMonthlyBalance")
            .WithOpenApi();
        }
    }
}
