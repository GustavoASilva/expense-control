namespace ExpenseControl.Api.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public TransactionType Type { get; set; }
    public string? IconName { get; set; }

    /// <summary>
    /// Null for default/global categories seeded at startup.
    /// Set to a household ID for user-created, household-specific categories.
    /// </summary>
    public Guid? HouseholdId { get; set; }
    public Household? Household { get; set; }
}
