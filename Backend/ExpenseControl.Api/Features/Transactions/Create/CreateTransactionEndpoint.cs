using System.Security.Claims;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Auth;
using ExpenseControl.Api.Persistence;

namespace ExpenseControl.Api.Features.Transactions.Create;

public record CreateTransactionRequest(
    string Description,
    decimal Amount,
    DateOnly Date,
    Guid CategoryId,
    TransactionType Type,
    string? Notes
);

public static class CreateTransactionEndpoint
{
    public static void MapCreateTransactionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/transactions", async (
            CreateTransactionRequest request,
            ClaimsPrincipal user,
            ExpenseDbContext db) =>
        {
            var householdId = user.GetHouseholdId();

            var category = await db.Categories.FindAsync(request.CategoryId);
            if (category == null)
                return Results.NotFound("Category not found");

            var household = await db.Households.FindAsync(householdId);
            if (household == null)
                return Results.NotFound("Household not found");

            Transaction transaction = request.Type switch
            {
                TransactionType.Expense => new Expense(),
                TransactionType.Income => new Income(),
                _ => throw new ArgumentException("Invalid transaction type")
            };

            transaction.Id = Guid.NewGuid();
            transaction.Description = request.Description;
            transaction.Amount = request.Amount;
            transaction.Date = request.Date;
            transaction.CategoryId = request.CategoryId;
            transaction.Notes = request.Notes ?? string.Empty;
            transaction.HouseholdId = householdId;

            db.Transactions.Add(transaction);
            await db.SaveChangesAsync();

            return Results.Created($"/api/transactions/{transaction.Id}", transaction.ToResponse());
        })
        .WithName("CreateTransaction")
        .WithOpenApi();
    }
}
