namespace ExpenseControl.Api.Features.Balance;

public record MonthlyBalanceResponse(
    int Year,
    IEnumerable<MonthSummaryResponse> Months,
    bool HasTransactions,
    decimal TotalIncome,
    decimal TotalExpenses
);

public record MonthSummaryResponse(
    int Month,
    string MonthName,
    decimal Income,
    decimal Expenses,
    decimal Balance,
    bool HasTransactions
);
