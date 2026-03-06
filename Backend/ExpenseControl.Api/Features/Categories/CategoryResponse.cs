using ExpenseControl.Api.Entities;

namespace ExpenseControl.Api.Features.Categories;

public record CategoryResponse(
    Guid Id,
    string? Name,
    string? Description,
    TransactionType Type,
    string? IconName,
    bool IsDefault
);

public static class CategoryMappingExtensions
{
    public static CategoryResponse ToResponse(this Category category)
    {
        return new CategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.Type,
            category.IconName,
            category.HouseholdId is null
        );
    }

    public static List<CategoryResponse> ToResponse(this IEnumerable<Category> categories)
    {
        return categories.Select(c => c.ToResponse()).ToList();
    }
}
