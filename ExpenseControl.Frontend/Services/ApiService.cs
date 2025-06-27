using System.Net.Http.Json;
using ExpenseControl.Frontend.Models;
using ExpenseControl.Frontend.Models.ApiResponses;
using Refit;

namespace ExpenseControl.Frontend.Services;

public class ApiService
{
    private readonly IApiService _api;

    public ApiService(IApiService api)
    {
        _api = api;
    }

    // Transactions
    public async Task<List<TransactionListItemModel>> GetTransactionsAsync(DateOnly? startDate = null, DateOnly? endDate = null)
    {
        try
        {
            var apiList = await _api.GetTransactionsAsync(startDate, endDate);
            return [.. apiList.Select(x => x.ToListItemModel())];
        }
        catch (ApiException ex)
        {
            throw new Exception("Failed to load transactions", ex);
        }
    }

    public async Task<TransactionFormModel?> GetTransactionAsync(Guid id)
    {
        try
        {
            var api = await _api.GetTransactionAsync(id);
            return api?.ToFormModel();
        }
        catch (ApiException ex)
        {
            throw new Exception("Failed to load transaction", ex);
        }
    }

    public async Task<TransactionFormModel> CreateTransactionAsync(TransactionFormModel transaction)
    {
        try
        {
            // Map to API request if needed, here assuming same as form model
            var api = await _api.CreateTransactionAsync(transaction);
            return api.ToFormModel();
        }
        catch (ApiException ex)
        {
            throw new Exception("Failed to create transaction", ex);
        }
    }

    public async Task<TransactionFormModel> UpdateTransactionAsync(Guid id, TransactionFormModel transaction)
    {
        try
        {
            var api = await _api.UpdateTransactionAsync(id, transaction);
            return api.ToFormModel();
        }
        catch (ApiException ex)
        {
            throw new Exception("Failed to update transaction", ex);
        }
    }

    public async Task DeleteTransactionAsync(Guid id)
    {
        try
        {
            await _api.DeleteTransactionAsync(id);
        }
        catch (ApiException ex)
        {
            throw new Exception("Failed to delete transaction", ex);
        }
    }

    // Categories
    public async Task<List<CategoryModel>> GetCategoriesAsync()
    {
        try
        {
            var apiList = await _api.GetCategoriesAsync();
            return [.. apiList.Select(x => x.ToCategoryModel())];
        }
        catch (ApiException ex)
        {
            throw new Exception("Failed to load categories", ex);
        }
    }

    // Balance
    public async Task<BalanceModel> GetBalanceAsync(DateOnly? startDate = null, DateOnly? endDate = null)
    {
        try
        {
            var api = await _api.GetBalanceAsync(startDate, endDate);
            return api.ToBalanceModel();
        }
        catch (ApiException ex)
        {
            throw new Exception("Failed to load balance", ex);
        }
    }

    public async Task<List<CategoryBalanceModel>> GetBalanceByCategoryAsync(DateOnly? startDate = null, DateOnly? endDate = null)
    {
        try
        {
            var api = await _api.GetBalanceByCategoryAsync(startDate, endDate);
            return [.. api.Categories.Select(x => x.ToCategoryBalanceModel())];
        }
        catch (ApiException ex)
        {
            throw new Exception("Failed to load balance by category", ex);
        }
    }

    public async Task<MonthlyBalanceModel> GetMonthlyBalanceAsync(int? year = null)
    {
        try
        {
            return await _api.GetMonthlyBalanceAsync(year);
        }
        catch (ApiException ex)
        {
            throw new Exception("Failed to load monthly balance", ex);
        }
    }

    // Budgets
    public async Task<List<BudgetModel>> GetBudgetsAsync(int? year = null, int? month = null, Guid? categoryId = null)
    {
        try
        {
            var apiList = await _api.GetBudgetsAsync(year, month, categoryId);
            return [.. apiList.Select(x => x.ToBudgetModel())];
        }
        catch (ApiException ex)
        {
            throw new Exception("Failed to load budgets", ex);
        }
    }

    public async Task CreateOrUpdateBudgetAsync(BudgetModel budget)
    {
        try
        {
            var api = await _api.CreateOrUpdateBudgetAsync(budget);
            return;
        }
        catch (ApiException ex)
        {
            throw new Exception("Failed to create or update budget", ex);
        }
    }

    public async Task<BudgetUsageModel> GetBudgetUsageAsync(Guid categoryId, int year, int month)
    {
        try
        {
            var api = await _api.GetBudgetUsageAsync(categoryId, year, month);
            return api.ToBudgetUsageModel();
        }
        catch (ApiException ex)
        {
            throw new Exception("Failed to get budget usage", ex);
        }
    }
}