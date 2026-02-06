using ExpenseControl.Api.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;

namespace ExpenseControl.Api.Features.Transactions.Create
{
    public record CreateTransactionCommand(
        string Description,
        decimal Amount,
        DateOnly Date,
        Guid CategoryId,
        TransactionType Type,
        string? Notes,
        Guid HouseholdId
    );

    public static class CreateTransactionEndpoint
    {
        public static void MapCreateTransactionEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/transactions", async (
                CreateTransactionCommand command,
                ExpenseControl.Api.Persistence.ExpenseDbContext db) =>
            {
                var category = await db.Categories.FindAsync(command.CategoryId);
                if (category == null)
                    return Results.NotFound("Category not found");

                var household = await db.Households.FindAsync(command.HouseholdId);
                if (household == null)
                    return Results.NotFound("Household not found");

                Transaction transaction = command.Type switch
                {
                    TransactionType.Expense => new Expense(),
                    TransactionType.Income => new Income(),
                    _ => throw new ArgumentException("Invalid transaction type")
                };

                transaction.Id = Guid.NewGuid();
                transaction.Description = command.Description;
                transaction.Amount = command.Amount;
                transaction.Date = command.Date;
                transaction.CategoryId = command.CategoryId;
                transaction.Notes = command.Notes ?? string.Empty;
                transaction.HouseholdId = command.HouseholdId;

                db.Transactions.Add(transaction);
                await db.SaveChangesAsync();

                return Results.Created($"/api/transactions/{transaction.Id}", transaction);
            })
            .WithName("CreateTransaction")
            .WithOpenApi();
        }
    }
}
