using ExpenseControl.Frontend.Models;
using ExpenseControl.Frontend.Models.ApiResponses;
using Refit;

namespace ExpenseControl.Frontend.Services;

public interface IApiService
{
    // Transactions
    [Get("/api/transactions")]
    Task<List<TransactionApiResponse>> GetTransactionsAsync([Query] DateOnly? startDate = null, [Query] DateOnly? endDate = null);

    [Get("/api/transactions/{id}")]
    Task<TransactionApiResponse?> GetTransactionAsync(Guid id);

    [Post("/api/transactions")]
    Task<TransactionApiResponse> CreateTransactionAsync([Body] TransactionFormModel transaction);

    [Delete("/api/transactions/{id}")]
    Task DeleteTransactionAsync(Guid id);

    [Patch("/api/transactions/{id}")]
    Task<TransactionApiResponse> UpdateTransactionAsync(Guid id, [Body] TransactionFormModel transaction);

    // Categories
    [Get("/api/categories")]
    Task<List<CategoryApiResponse>> GetCategoriesAsync();

    // Balance
    [Get("/api/balance")]
    Task<BalanceApiResponse> GetBalanceAsync([Query] DateOnly? startDate = null, [Query] DateOnly? endDate = null);

    [Get("/api/balance/by-category")]
    Task<BalanceByCategoryApiResponse> GetBalanceByCategoryAsync([Query] DateOnly? startDate = null, [Query] DateOnly? endDate = null);

    [Get("/api/balance/monthly")]
    Task<MonthlyBalanceModel> GetMonthlyBalanceAsync([Query] int? year = null);

    // Budgets
    [Get("/api/budgets")]
    Task<List<BudgetApiResponse>> GetBudgetsAsync([Query] int? year = null, [Query] int? month = null, [Query] Guid? categoryId = null);

    [Post("/api/budgets")]
    Task<BudgetApiResponse> CreateOrUpdateBudgetAsync([Body] BudgetModel budget);

    [Get("/api/budgets/usage")]
    Task<BudgetUsageApiResponse> GetBudgetUsageAsync([Query] Guid categoryId, [Query] int year, [Query] int month);
}
