using ExpenseControl.Api.Entities;

namespace ExpenseControl.Api.Features.Transactions.Update;

public record UpdateTransactionRequest(
    string? Description,
    decimal Amount,
    DateOnly Date,
    Guid CategoryId,
    TransactionType Type,
    string? Notes
);
