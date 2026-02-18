namespace ExpenseControl.Api.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public TransactionType Type { get; set; }
    public string? IconName { get; set; }
}
