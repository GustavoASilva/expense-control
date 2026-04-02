namespace ExpenseControl.Api.Features.Savings.Update;

public record UpdateSavingRequest(
    string Name,
    string? Description,
    decimal CurrentAmount,
    decimal? TargetAmount
);
