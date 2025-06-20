using System.Net.Http.Json;
using ExpenseControl.Frontend.Models;

namespace ExpenseControl.Frontend.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "api";

    public ApiService(HttpClient http)
    {
        _http = http;
    }

    // Transactions
    public async Task<List<Transaction>> GetTransactionsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var url = $"{BaseUrl}/transactions";
        if (startDate.HasValue) url += $"?startDate={startDate.Value:yyyy-MM-dd}";
        if (endDate.HasValue) url += $"{(startDate.HasValue ? "&" : "?")}endDate={endDate.Value:yyyy-MM-dd}";
        return await _http.GetFromJsonAsync<List<Transaction>>(url) ?? new();
    }

    public async Task<Transaction?> GetTransactionAsync(Guid id)
    {
        return await _http.GetFromJsonAsync<Transaction>($"{BaseUrl}/transactions/{id}");
    }

    public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
    {
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/transactions", transaction);
        return await response.Content.ReadFromJsonAsync<Transaction>() ?? throw new Exception("Failed to create transaction");
    }

    public async Task DeleteTransactionAsync(Guid id)
    {
        await _http.DeleteAsync($"{BaseUrl}/transactions/{id}");
    }

    // Categories
    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _http.GetFromJsonAsync<List<Category>>($"{BaseUrl}/categories") ?? new();
    }

    // Balance
    public async Task<Balance> GetBalanceAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var url = $"{BaseUrl}/balance";
        if (startDate.HasValue) url += $"?startDate={startDate.Value:yyyy-MM-dd}";
        if (endDate.HasValue) url += $"{(startDate.HasValue ? "&" : "?")}endDate={endDate.Value:yyyy-MM-dd}";
        return await _http.GetFromJsonAsync<Balance>(url) ?? new();
    }

    public async Task<List<CategoryBalance>> GetBalanceByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var url = $"{BaseUrl}/balance/by-category";
        if (startDate.HasValue) url += $"?startDate={startDate.Value:yyyy-MM-dd}";
        if (endDate.HasValue) url += $"{(startDate.HasValue ? "&" : "?")}endDate={endDate.Value:yyyy-MM-dd}";
        var response = await _http.GetFromJsonAsync<BalanceByCategoryResponse>(url);
        return response?.Categories.ToList() ?? new();
    }

    public async Task<MonthlyBalance> GetMonthlyBalanceAsync(int? year = null)
    {
        var url = $"{BaseUrl}/balance/monthly";
        if (year.HasValue) url += $"?year={year.Value}";
        return await _http.GetFromJsonAsync<MonthlyBalance>(url) ?? new();
    }
}

public class BalanceByCategoryResponse
{
    public IEnumerable<CategoryBalance> Categories { get; set; } = Array.Empty<CategoryBalance>();
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public bool HasTransactions { get; set; }
}
