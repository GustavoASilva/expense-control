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
    public async Task<List<TransactionListItemModel>> GetTransactionsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var apiList = await _api.GetTransactionsAsync(startDate, endDate);
            return apiList.Select(x => x.ToListItemModel()).ToList();
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
    public async Task<List<Category>> GetCategoriesAsync()
    {
        try
        {
            var apiList = await _api.GetCategoriesAsync();
            return apiList.Select(x => x.ToCategoryModel()).ToList();
        }
        catch (ApiException ex)
        {
            throw new Exception("Failed to load categories", ex);
        }
    }

    // Balance
    public async Task<Balance> GetBalanceAsync(DateTime? startDate = null, DateTime? endDate = null)
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

    public async Task<List<CategoryBalance>> GetBalanceByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var api = await _api.GetBalanceByCategoryAsync(startDate, endDate);
            return api.Categories.Select(x => x.ToCategoryBalanceModel()).ToList();
        }
        catch (ApiException ex)
        {
            throw new Exception("Failed to load balance by category", ex);
        }
    }

    public async Task<MonthlyBalance> GetMonthlyBalanceAsync(int? year = null)
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
}