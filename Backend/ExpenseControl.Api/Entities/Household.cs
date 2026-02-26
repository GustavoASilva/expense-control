namespace ExpenseControl.Api.Entities;

public class Household
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unique invite identifier for future household invite/join functionality.
    /// Generated automatically during household creation.
    /// </summary>
    public Guid InviteId { get; set; }

    public DateTime CreatedAt { get; set; }
}
