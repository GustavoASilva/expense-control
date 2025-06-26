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

    public static Category ToCategoryModel(this CategoryApiResponse api)
    {
        return new Category
        {
            Id = api.Id,
            Name = api.Name,
            Description = api.Description,
            Type = api.Type,
            IconName = api.IconName,
            ColorCode = api.ColorCode
        };
    }

    public static Balance ToBalanceModel(this BalanceApiResponse api)
    {
        return new Balance
        {
            Income = api.Income,
            Expenses = api.Expenses,
            PeriodStart = api.PeriodStart,
            PeriodEnd = api.PeriodEnd,
            HasTransactions = api.HasTransactions,
            CurrentBalance = api.Balance
        };
    }

    public static CategoryBalance ToCategoryBalanceModel(this CategoryBalanceApiResponse api)
    {
        return new CategoryBalance
        {
            CategoryName = api.CategoryName,
            TransactionType = api.TransactionType,
            Total = api.Total,
            Count = api.Count
        };
    }
}
