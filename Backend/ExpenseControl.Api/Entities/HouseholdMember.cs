namespace ExpenseControl.Api.Entities;

/// <summary>
/// Represents the membership relationship between a user and a household.
/// Supports multiple users per household for future invite functionality.
/// </summary>
public class HouseholdMember
{
    public Guid Id { get; set; }
    public Guid HouseholdId { get; set; }
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Optional role for future permission levels (e.g., "Owner", "Member").
    /// </summary>
    public string? Role { get; set; }

    public DateTime JoinedAt { get; set; }
    public Household Household { get; set; } = null!;
}
