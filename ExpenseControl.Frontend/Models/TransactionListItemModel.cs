using System;

namespace ExpenseControl.Frontend.Models;

public class TransactionListItemModel
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TransactionType Type { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; }
}
