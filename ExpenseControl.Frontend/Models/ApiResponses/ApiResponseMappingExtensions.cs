using ExpenseControl.Frontend.Models.ApiResponses;

namespace ExpenseControl.Frontend.Models;

public static class ApiResponseMappingExtensions
{
    public static TransactionListItemModel ToListItemModel(this TransactionApiResponse api)
    {
        return new TransactionListItemModel
        {
            Id = api.Id,
            Amount = api.Amount,
            Description = api.Description,
            Date = api.Date,
            Type = api.Type,
            CategoryId = api.CategoryId,
            CategoryName = api.Category?.Name ?? string.Empty
        };
    }

    public static TransactionFormModel ToFormModel(this TransactionApiResponse api)
    {
        return new TransactionFormModel
        {
            Id = api.Id,
            Amount = api.Amount,
            Description = api.Description,
            Date = api.Date,
            Type = api.Type,
            CategoryId = api.CategoryId,
            Notes = api.Notes
        };
    }

    public static CategoryModel ToCategoryModel(this CategoryApiResponse api)
    {
        return new CategoryModel
        {
            Id = api.Id,
            Name = api.Name ?? string.Empty,
            Description = api.Description,
            Type = api.Type,
            IconName = api.IconName ?? string.Empty
        };
    }

    public static BalanceModel ToBalanceModel(this BalanceApiResponse api)
    {
        return new BalanceModel
        {
            Income = api.Income,
            Expenses = api.Expenses,
            CurrentBalance = api.Balance
        };
    }

    public static CategoryBalanceModel ToCategoryBalanceModel(this CategoryBalanceApiResponse api)
    {
        return new CategoryBalanceModel
        {
            CategoryName = api.CategoryName ?? string.Empty,
            TransactionType = api.TransactionType,
            Total = api.Total,
            Count = api.Count
        };
    }

    public static BudgetModel ToBudgetModel(this BudgetApiResponse api)
    {
        return new BudgetModel
        {
            Id = api.Id,
            CategoryId = api.CategoryId,
            CategoryName = api.Category.Name ?? string.Empty,
            Amount = api.Amount,
            Month = api.Month,
            Year = api.Year,
            CreatedAt = api.CreatedAt,
            UpdatedAt = api.UpdatedAt
        };
    }

    public static BudgetUsageModel ToBudgetUsageModel(this BudgetUsageApiResponse api)
    {
        return new BudgetUsageModel
        {
            Amount = api.Amount,
            Usage = api.Usage,
            Percent = api.Percent
        };
    }
}
