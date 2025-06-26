using ExpenseControl.Frontend.Models;
using ExpenseControl.Frontend.Models.ApiResponses;
using Refit;

namespace ExpenseControl.Frontend.Services;

public interface IApiService
{
    // Transactions
    [Get("/api/transactions")]
    Task<List<TransactionApiResponse>> GetTransactionsAsync([Query] DateTime? startDate = null, [Query] DateTime? endDate = null);

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
    Task<BalanceApiResponse> GetBalanceAsync([Query] DateTime? startDate = null, [Query] DateTime? endDate = null);

    [Get("/api/balance/by-category")]
    Task<BalanceByCategoryApiResponse> GetBalanceByCategoryAsync([Query] DateTime? startDate = null, [Query] DateTime? endDate = null);

    [Get("/api/balance/monthly")]
    Task<MonthlyBalance> GetMonthlyBalanceAsync([Query] int? year = null);
}
