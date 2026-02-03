namespace ExpenseControl.Frontend.Models;

public class CategoryBalanceModel
{
    public string CategoryName { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
    public decimal Total { get; set; }
    public int Count { get; set; }
}
