using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Categories;

namespace ExpenseControl.Api.Features.Budgets;

public record BudgetResponse(
    Guid Id,
    Guid CategoryId,
    CategoryResponse? Category,
    decimal Amount,
    int Month,
    int Year
);

public static class BudgetMappingExtensions
{
    public static BudgetResponse ToResponse(this Budget budget)
    {
        return new BudgetResponse(
            budget.Id,
            budget.CategoryId,
            budget.Category?.ToResponse(),
            budget.Amount,
            budget.Month,
            budget.Year
        );
    }

    public static List<BudgetResponse> ToResponse(this IEnumerable<Budget> budgets)
    {
        return budgets.Select(b => b.ToResponse()).ToList();
    }
}
