using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Categories;

namespace ExpenseControl.Api.Features.Savings;

public record SavingResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal CurrentAmount,
    decimal? TargetAmount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public static class SavingMappingExtensions
{
    public static SavingResponse ToResponse(this Saving saving)
    {
        return new SavingResponse(
            saving.Id,
            saving.Name,
            saving.Description,
            saving.CurrentAmount,
            saving.TargetAmount,
            saving.CreatedAt,
            saving.UpdatedAt
        );
    }

    public static List<SavingResponse> ToResponse(this IEnumerable<Saving> savings)
    {
        return savings.Select(s => s.ToResponse()).ToList();
    }
}
