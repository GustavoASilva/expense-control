using System.Security.Claims;

namespace ExpenseControl.Api.Features.Auth;

/// <summary>
/// Extension methods for extracting claims from an authenticated <see cref="ClaimsPrincipal"/>.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// The claim type used to store the household identifier.
    /// Populated by <see cref="HouseholdClaimsTransformation"/> from the database
    /// when not already present in the JWT token.
    /// </summary>
    public const string HouseholdIdClaimType = "householdId";

    /// <summary>
    /// The raw JWT <c>sub</c> claim type used as a fallback for user identification
    /// when <see cref="ClaimTypes.NameIdentifier"/> is not mapped (e.g., Cognito tokens).
    /// </summary>
    public const string SubClaimType = "sub";

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
    /// Returns the household identifier if present, or <c>null</c> when the claim is missing.
    /// Use this for endpoints that can operate without a household context.
    /// </summary>
    public static Guid? TryGetHouseholdId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(HouseholdIdClaimType);
        if (claim is not null && Guid.TryParse(claim.Value, out var householdId))
            return householdId;

        return null;
    }

    /// <summary>
    /// Returns the user identifier from the principal's NameIdentifier or <c>sub</c> claim.
    /// Checks <see cref="ClaimTypes.NameIdentifier"/> first, then falls back to the
    /// raw JWT <c>sub</c> claim for compatibility with Cognito tokens.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when neither claim is present or both are empty.
    /// </exception>
    public static string GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier)
                 ?? principal.FindFirst(SubClaimType);
        if (claim is null || string.IsNullOrWhiteSpace(claim.Value))
            throw new InvalidOperationException("The user identifier claim is missing in the token.");

        return claim.Value;
    }
}
