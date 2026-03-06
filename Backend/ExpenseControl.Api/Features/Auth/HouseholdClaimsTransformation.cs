using System.Security.Claims;
using ExpenseControl.Api.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Auth;

/// <summary>
/// Enriches the authenticated principal with a <c>householdId</c> claim
/// resolved from the <see cref="Entities.HouseholdMember"/> table.
/// This removes the need for the JWT token itself to carry household information,
/// which is critical for Cognito access tokens that do not include custom attributes.
/// </summary>
public class HouseholdClaimsTransformation(ExpenseDbContext db) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
            return principal;

        // Skip if household claim already present (e.g., from mock auth)
        if (principal.FindFirst(ClaimsPrincipalExtensions.HouseholdIdClaimType) is not null)
            return principal;

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? principal.FindFirst(ClaimsPrincipalExtensions.SubClaimType)?.Value;

        if (string.IsNullOrWhiteSpace(userId))
            return principal;

        var householdId = await db.HouseholdMembers
            .Where(m => m.UserId == userId)
            .Select(m => m.HouseholdId)
            .FirstOrDefaultAsync();

        if (householdId != Guid.Empty)
        {
            if (principal.Identity is ClaimsIdentity identity)
            {
                identity.AddClaim(new Claim(
                    ClaimsPrincipalExtensions.HouseholdIdClaimType,
                    householdId.ToString()));
            }
        }

        return principal;
    }
}
