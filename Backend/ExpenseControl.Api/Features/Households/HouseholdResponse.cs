using ExpenseControl.Api.Entities;

namespace ExpenseControl.Api.Features.Households;

public record HouseholdResponse(
    Guid Id,
    string Name,
    DateTime CreatedAt
);

public static class HouseholdMappingExtensions
{
    public static HouseholdResponse ToResponse(this Household household)
    {
        return new HouseholdResponse(
            household.Id,
            household.Name,
            household.CreatedAt
        );
    }

    public static List<HouseholdResponse> ToResponse(this IEnumerable<Household> households)
    {
        return households.Select(h => h.ToResponse()).ToList();
    }
}
