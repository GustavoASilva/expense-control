namespace ExpenseControl.Api.Features.Balance;

public record BalanceResponse(
    decimal Income,
    decimal Expenses,
    decimal Balance,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    bool HasTransactions
);
