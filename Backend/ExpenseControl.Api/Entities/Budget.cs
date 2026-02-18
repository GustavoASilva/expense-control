namespace ExpenseControl.Api.Entities;

public class Budget
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public decimal Amount { get; set; }
    public int Month { get; set; } // 1-12
    public int Year { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid HouseholdId { get; set; }
    public Household Household { get; set; } = null!;
}
