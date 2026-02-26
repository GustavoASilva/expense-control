using System.Security.Claims;
using ExpenseControl.Api.Features.Auth;

namespace ExpenseControl.Api.Tests.Features.Auth;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetUserId_WithValidClaim_ReturnsUserId()
    {
        var userId = "test-user-123";
        var principal = CreatePrincipal(ClaimTypes.NameIdentifier, userId);

        var result = principal.GetUserId();

        Assert.Equal(userId, result);
    }

    [Fact]
    public void GetUserId_WithMissingClaim_ThrowsInvalidOperationException()
    {
        var principal = CreatePrincipal("other-claim", "value");

        Assert.Throws<InvalidOperationException>(() => principal.GetUserId());
    }

    [Fact]
    public void GetUserId_WithEmptyClaim_ThrowsInvalidOperationException()
    {
        var principal = CreatePrincipal(ClaimTypes.NameIdentifier, "");

        Assert.Throws<InvalidOperationException>(() => principal.GetUserId());
    }

    [Fact]
    public void GetHouseholdId_WithValidClaim_ReturnsHouseholdId()
    {
        var householdId = Guid.NewGuid();
        var principal = CreatePrincipal(ClaimsPrincipalExtensions.HouseholdIdClaimType, householdId.ToString());

        var result = principal.GetHouseholdId();

        Assert.Equal(householdId, result);
    }

    [Fact]
    public void GetHouseholdId_WithMissingClaim_ThrowsInvalidOperationException()
    {
        var principal = CreatePrincipal("other-claim", "value");

        Assert.Throws<InvalidOperationException>(() => principal.GetHouseholdId());
    }

    [Fact]
    public void GetHouseholdId_WithInvalidGuid_ThrowsInvalidOperationException()
    {
        var principal = CreatePrincipal(ClaimsPrincipalExtensions.HouseholdIdClaimType, "not-a-guid");

        Assert.Throws<InvalidOperationException>(() => principal.GetHouseholdId());
    }

    private static ClaimsPrincipal CreatePrincipal(string claimType, string claimValue)
    {
        var identity = new ClaimsIdentity([new Claim(claimType, claimValue)], "Test");

        return new ClaimsPrincipal(identity);
    }
}
