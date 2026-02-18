using System.Security.Claims;
using System.Text.Encodings.Web;
using ExpenseControl.Api.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace ExpenseControl.Api.Features.Auth;

public class MockAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public MockAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "mock-user-id"),
            new Claim(ClaimTypes.Name, "Mock User"),
            new Claim(ClaimTypes.Email, "mock@example.com"),
            new Claim(ClaimsPrincipalExtensions.HouseholdIdClaimType, DevSeedData.MockHouseholdId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "Mock");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Mock");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
