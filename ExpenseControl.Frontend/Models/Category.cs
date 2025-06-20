namespace ExpenseControl.Frontend.Models;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TransactionType Type { get; set; }
    public string IconName { get; set; } = string.Empty;
    public string ColorCode { get; set; } = string.Empty;
}
