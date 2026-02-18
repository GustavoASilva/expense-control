using System.Security.Claims;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Auth;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Transactions.List;

public static class ListTransactionsEndpoint
{
    public static void MapListTransactionsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/transactions", async (
            ExpenseDbContext db,
            ClaimsPrincipal user,
            TransactionType? type,
            Guid? categoryId,
            DateOnly? startDate,
            DateOnly? endDate,
            int? limit,
            int? offset) =>
        {
            var householdId = user.GetHouseholdId();
            var query = db.Transactions
                .Include(t => t.Category)
                .Where(t => t.HouseholdId == householdId);

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
