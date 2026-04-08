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

            // Project to only the fields needed for aggregation, then load into memory.
            // Amount is stored as encrypted text and cannot be aggregated directly in the database.
            var summaries = await query
                .Select(t => new { t.Type, t.Amount })
                .ToListAsync();

            var income = summaries
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);
            var expenses = summaries
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            return Results.Ok(new BalanceResponse(
                income,
                expenses,
                income - expenses,
                periodStart,
                periodEnd,
                summaries.Count > 0
            ));
        })
        .WithName("GetBalance")
        .WithOpenApi();
    }
}
