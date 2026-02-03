using System.Collections.Generic;

namespace ExpenseControl.Frontend.Models;

public class MonthlyBalanceModel
{
    public int Year { get; set; }
    public List<MonthlyBalanceMonthModel> Months { get; set; } = new();
    public bool HasTransactions { get; set; }
}

public class MonthlyBalanceMonthModel
{
    public string MonthName { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
}
