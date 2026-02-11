namespace ExpenseControl.Api.Entities;

public static class WellKnownRoles
{
    public static readonly Guid AdminRoleId = new("a1b2c3d4-e5f6-4a5b-8c9d-0e1f2a3b4c5d");
    public static readonly Guid MemberRoleId = new("b2c3d4e5-f6a7-5b6c-9d0e-1f2a3b4c5d6e");

    public const string Admin = "Admin";
    public const string Member = "Member";
}
