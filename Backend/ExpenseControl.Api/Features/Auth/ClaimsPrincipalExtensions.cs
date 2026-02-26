using System.Security.Claims;

namespace ExpenseControl.Api.Features.Auth;

/// <summary>
/// Extension methods for extracting claims from an authenticated <see cref="ClaimsPrincipal"/>.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// The claim type used to store the household identifier in the JWT.
    /// For AWS Cognito, configure the user pool to include a custom attribute
    /// named <c>householdId</c> in the access token.
    /// </summary>
    public const string HouseholdIdClaimType = "householdId";

    /// <summary>
    /// Returns the household identifier stored in the principal's claims.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the claim is missing or does not contain a valid <see cref="Guid"/>.
    /// </exception>
    public static Guid GetHouseholdId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(HouseholdIdClaimType);
        if (claim is null || !Guid.TryParse(claim.Value, out var householdId))
            throw new InvalidOperationException("The 'householdId' claim is missing or invalid in the token.");

        return householdId;
    }

    /// <summary>
    /// Returns the user identifier from the principal's NameIdentifier claim.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the NameIdentifier claim is missing or empty.
    /// </exception>
    public static string GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null || string.IsNullOrWhiteSpace(claim.Value))
            throw new InvalidOperationException("The user identifier claim is missing in the token.");

        return claim.Value;
    }
}
