using System.Security.Claims;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Auth;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Tests.Features.Auth;

public class HouseholdClaimsTransformationTests : IDisposable
{
    private readonly ExpenseDbContext _db;
    private readonly HouseholdClaimsTransformation _transformation;

    public HouseholdClaimsTransformationTests()
    {
        var options = new DbContextOptionsBuilder<ExpenseDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new ExpenseDbContext(options);
        _transformation = new HouseholdClaimsTransformation(_db);
    }

    [Fact]
    public async Task TransformAsync_WhenNotAuthenticated_ReturnsPrincipalUnchanged()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var result = await _transformation.TransformAsync(principal);

        Assert.Null(result.FindFirst(ClaimsPrincipalExtensions.HouseholdIdClaimType));
    }

    [Fact]
    public async Task TransformAsync_WhenHouseholdClaimAlreadyPresent_SkipsLookup()
    {
        var existingHouseholdId = Guid.NewGuid();
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(ClaimsPrincipalExtensions.HouseholdIdClaimType, existingHouseholdId.ToString())
        ], "Test");
        var principal = new ClaimsPrincipal(identity);

        var result = await _transformation.TransformAsync(principal);

        var claim = result.FindFirst(ClaimsPrincipalExtensions.HouseholdIdClaimType);
        Assert.NotNull(claim);
        Assert.Equal(existingHouseholdId.ToString(), claim.Value);
    }

    [Fact]
    public async Task TransformAsync_WhenUserHasHouseholdMembership_AddsHouseholdClaim()
    {
        var userId = "cognito-user-abc";
        var householdId = Guid.NewGuid();
        await SeedMembership(userId, householdId);

        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId)], "Bearer");
        var principal = new ClaimsPrincipal(identity);

        var result = await _transformation.TransformAsync(principal);

        var claim = result.FindFirst(ClaimsPrincipalExtensions.HouseholdIdClaimType);
        Assert.NotNull(claim);
        Assert.Equal(householdId.ToString(), claim.Value);
    }

    [Fact]
    public async Task TransformAsync_WhenUserHasNoMembership_DoesNotAddClaim()
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "orphan-user")], "Bearer");
        var principal = new ClaimsPrincipal(identity);

        var result = await _transformation.TransformAsync(principal);

        Assert.Null(result.FindFirst(ClaimsPrincipalExtensions.HouseholdIdClaimType));
    }

    [Fact]
    public async Task TransformAsync_WhenUserIdInSubClaim_ResolvesHousehold()
    {
        var userId = "cognito-sub-uuid";
        var householdId = Guid.NewGuid();
        await SeedMembership(userId, householdId);

        // Simulate Cognito token with raw "sub" claim (no NameIdentifier mapping)
        var identity = new ClaimsIdentity(
            [new Claim(ClaimsPrincipalExtensions.SubClaimType, userId)], "Bearer");
        var principal = new ClaimsPrincipal(identity);

        var result = await _transformation.TransformAsync(principal);

        var claim = result.FindFirst(ClaimsPrincipalExtensions.HouseholdIdClaimType);
        Assert.NotNull(claim);
        Assert.Equal(householdId.ToString(), claim.Value);
    }

    [Fact]
    public async Task TransformAsync_WhenNoUserIdClaim_ReturnsPrincipalUnchanged()
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Email, "user@example.com")], "Bearer");
        var principal = new ClaimsPrincipal(identity);

        var result = await _transformation.TransformAsync(principal);

        Assert.Null(result.FindFirst(ClaimsPrincipalExtensions.HouseholdIdClaimType));
    }

    private async Task SeedMembership(string userId, Guid householdId)
    {
        _db.Households.Add(new Household
        {
            Id = householdId,
            Name = "Test Household",
            InviteId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        });

        _db.HouseholdMembers.Add(new HouseholdMember
        {
            Id = Guid.NewGuid(),
            HouseholdId = householdId,
            UserId = userId,
            Role = "Owner",
            JoinedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
