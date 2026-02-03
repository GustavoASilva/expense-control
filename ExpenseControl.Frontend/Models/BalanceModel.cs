namespace ExpenseControl.Frontend.Models;

public class BalanceModel
{
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal CurrentBalance { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public bool HasTransactions { get; set; }
}

public class CategoryBalance
{
    public string CategoryName { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
    public decimal Total { get; set; }
    public int Count { get; set; }
}

public class MonthlyBalance
{
    public int Year { get; set; }
    public List<MonthSummary> Months { get; set; } = new();
    public bool HasTransactions { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal YearlyBalance => TotalIncome - TotalExpenses;
}

public class MonthSummary
{
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal Balance { get; set; }
    public bool HasTransactions { get; set; }
}
