namespace ExpenseControl.Api.Features.Budgets.Create;

public record CreateBudgetRequest(
    Guid CategoryId,
    decimal Amount,
    int Month,
    int Year
);
