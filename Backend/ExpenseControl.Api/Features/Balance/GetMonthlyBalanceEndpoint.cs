using System.Security.Claims;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Auth;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Balance;

public static class GetMonthlyBalanceEndpoint
{
    public static RouteHandlerBuilder MapGetMonthlyBalanceEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/api/balance/monthly", async (ExpenseDbContext db, ClaimsPrincipal user, int? year) =>
        {
            var householdId = user.GetHouseholdId();
            var targetYear = year ?? DateTime.UtcNow.Year;
            var currentMonth = DateTime.UtcNow.Month;

            var query = db.Transactions
                .Where(t => t.Date.Year == targetYear)
                .Where(t => t.HouseholdId == householdId);

            var transactions = await query.ToListAsync();

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

                    return new MonthSummaryResponse(
                        month,
                        new DateTime(targetYear, month, 1).ToString("MMMM"),
                        income,
                        expenses,
                        income - expenses,
                        monthTransactions.Any()
                    );
                })
                .OrderBy(m => m.Month);

            return Results.Ok(new MonthlyBalanceResponse(
                targetYear,
                monthlySummary,
                transactions.Any(),
                transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
            ));
        })
        .WithName("GetMonthlyBalance")
        .WithOpenApi();
    }
}
