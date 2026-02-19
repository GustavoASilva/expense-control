using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Categories;

namespace ExpenseControl.Api.Features.Transactions;

public record TransactionResponse(
    Guid Id,
    string? Description,
    decimal Amount,
    DateOnly Date,
    Guid CategoryId,
    TransactionType Type,
    string? Notes,
    CategoryResponse? Category
);

public static class TransactionMappingExtensions
{
    public static TransactionResponse ToResponse(this Transaction transaction)
    {
        return new TransactionResponse(
            transaction.Id,
            transaction.Description,
            transaction.Amount,
            transaction.Date,
            transaction.CategoryId,
            transaction.Type,
            transaction.Notes,
            transaction.Category?.ToResponse()
        );
    }

    public static List<TransactionResponse> ToResponse(this IEnumerable<Transaction> transactions)
    {
        return transactions.Select(t => t.ToResponse()).ToList();
    }
}
