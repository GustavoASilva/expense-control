using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Households;
using ExpenseControl.Api.Persistence;
using ExpenseControl.Api.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Tests.Features.Households;

public class JoinHouseholdEndpointTests
{
    [Fact]
    public async Task JoinHousehold_WithValidInviteId_AddsMemberAndReturnsHousehold()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = new Household
        {
            Id = Guid.NewGuid(),
            Name = "Test Household",
            InviteId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        db.Households.Add(household);
        await db.SaveChangesAsync();

        var userId = "new-user-id";

        var result = await ExecuteJoinHousehold(db, household.InviteId, userId);

        var okResult = Assert.IsType<Ok<HouseholdResponse>>(result);
        Assert.Equal(household.Id, okResult.Value!.Id);
        Assert.Equal("Test Household", okResult.Value.Name);

        var member = await db.HouseholdMembers.FirstOrDefaultAsync(m => m.UserId == userId);
        Assert.NotNull(member);
        Assert.Equal(household.Id, member.HouseholdId);
        Assert.Equal("Member", member.Role);
    }

    [Fact]
    public async Task JoinHousehold_WithInvalidInviteId_ReturnsNotFound()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var result = await ExecuteJoinHousehold(db, Guid.NewGuid(), "some-user");

        Assert.IsType<NotFound<string>>(result);
    }

    [Fact]
    public async Task JoinHousehold_WhenUserAlreadyHasHousehold_ReturnsConflict()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var existingHousehold = new Household
        {
            Id = Guid.NewGuid(),
            Name = "Existing Household",
            InviteId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        db.Households.Add(existingHousehold);

        var userId = "existing-user";
        db.HouseholdMembers.Add(new HouseholdMember
        {
            Id = Guid.NewGuid(),
            HouseholdId = existingHousehold.Id,
            UserId = userId,
            Role = "Owner",
            JoinedAt = DateTime.UtcNow
        });

        var targetHousehold = new Household
        {
            Id = Guid.NewGuid(),
            Name = "Target Household",
            InviteId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        db.Households.Add(targetHousehold);
        await db.SaveChangesAsync();

        var result = await ExecuteJoinHousehold(db, targetHousehold.InviteId, userId);

        Assert.IsType<Conflict<string>>(result);
    }

    private static async Task<IResult> ExecuteJoinHousehold(ExpenseDbContext db, Guid inviteId, string userId)
    {
        var existingMembership = await db.HouseholdMembers.AnyAsync(m => m.UserId == userId);
        if (existingMembership)
        {
            return Results.Conflict("You already belong to a household.");
        }

        var household = await db.Households.FirstOrDefaultAsync(h => h.InviteId == inviteId);
        if (household is null)
        {
            return Results.NotFound("No household found with this invite code.");
        }

        var member = new HouseholdMember
        {
            Id = Guid.NewGuid(),
            HouseholdId = household.Id,
            UserId = userId,
            Role = "Member",
            JoinedAt = DateTime.UtcNow
        };

        db.HouseholdMembers.Add(member);
        await db.SaveChangesAsync();

        return Results.Ok(household.ToResponse());
    }
}
