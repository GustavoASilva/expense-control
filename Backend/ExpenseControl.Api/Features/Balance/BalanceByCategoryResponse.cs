using ExpenseControl.Api.Entities;

namespace ExpenseControl.Api.Features.Balance;

public record BalanceByCategoryResponse(
    IEnumerable<CategoryBalanceResponse> Categories,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    bool HasTransactions
);

public record CategoryBalanceResponse(
    string? CategoryName,
    TransactionType TransactionType,
    decimal Total,
    int Count
);
