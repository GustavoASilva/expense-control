namespace ExpenseControl.Api.Features.Savings.Create;

public record CreateSavingRequest(
    string Name,
    string? Description,
    decimal CurrentAmount,
    decimal? TargetAmount
);
