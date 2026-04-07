namespace ExpenseControl.Api.Entities;

public class Saving
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal CurrentAmount { get; set; }
    public decimal? TargetAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid HouseholdId { get; set; }
    public Household Household { get; set; } = null!;
}
