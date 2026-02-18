using System.Security.Claims;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Auth;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Balance;

public static class GetBalanceEndpoint
{
    public static RouteHandlerBuilder MapGetBalanceEndpoint(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/api/balance", async (ExpenseDbContext db, ClaimsPrincipal user, DateOnly? startDate, DateOnly? endDate) =>
        {
            var householdId = user.GetHouseholdId();
            var query = db.Transactions
                .Where(t => t.HouseholdId == householdId);

            var periodStart = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow.Date.AddMonths(-1));
            var periodEnd = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);

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
