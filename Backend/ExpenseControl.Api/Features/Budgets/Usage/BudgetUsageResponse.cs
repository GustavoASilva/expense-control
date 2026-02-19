namespace ExpenseControl.Api.Features.Budgets.Usage;

public record BudgetUsageResponse(
    decimal Amount,
    decimal Usage,
    decimal Percent
);
