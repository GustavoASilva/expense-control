using System;

namespace ExpenseControl.Frontend.Models.ApiResponses;

public class BalanceApiResponse
{
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal Balance { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public bool HasTransactions { get; set; }
}
