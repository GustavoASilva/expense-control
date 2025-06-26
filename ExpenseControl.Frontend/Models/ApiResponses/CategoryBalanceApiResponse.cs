using System;

namespace ExpenseControl.Frontend.Models.ApiResponses;

public class CategoryBalanceApiResponse
{
    public string CategoryName { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Total { get; set; }
    public int Count { get; set; }
}
