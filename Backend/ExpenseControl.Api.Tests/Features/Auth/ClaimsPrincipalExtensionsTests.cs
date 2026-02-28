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

    [Fact]
    public void TryGetHouseholdId_WithValidClaim_ReturnsHouseholdId()
    {
        var householdId = Guid.NewGuid();
        var principal = CreatePrincipal(ClaimsPrincipalExtensions.HouseholdIdClaimType, householdId.ToString());

        var result = principal.TryGetHouseholdId();

        Assert.Equal(householdId, result);
    }

    [Fact]
    public void TryGetHouseholdId_WithMissingClaim_ReturnsNull()
    {
        var principal = CreatePrincipal("other-claim", "value");

        var result = principal.TryGetHouseholdId();

        Assert.Null(result);
    }

    [Fact]
    public void TryGetHouseholdId_WithInvalidGuid_ReturnsNull()
    {
        var principal = CreatePrincipal(ClaimsPrincipalExtensions.HouseholdIdClaimType, "not-a-guid");

        var result = principal.TryGetHouseholdId();

        Assert.Null(result);
    }

    [Fact]
    public void GetUserId_WithSubClaim_ReturnsUserId()
    {
        var userId = "cognito-sub-uuid";
        var principal = CreatePrincipal(ClaimsPrincipalExtensions.SubClaimType, userId);

        var result = principal.GetUserId();

        Assert.Equal(userId, result);
    }

    [Fact]
    public void GetUserId_PrefersNameIdentifierOverSub()
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "name-id-value"),
            new Claim(ClaimsPrincipalExtensions.SubClaimType, "sub-value")
        ], "Test");
        var principal = new ClaimsPrincipal(identity);

        var result = principal.GetUserId();

        Assert.Equal("name-id-value", result);
    }

    private static ClaimsPrincipal CreatePrincipal(string claimType, string claimValue)
    {
        var identity = new ClaimsIdentity([new Claim(claimType, claimValue)], "Test");

        return new ClaimsPrincipal(identity);
    }
}
