using System.Security.Claims;
using ExpenseControl.Api.Features.Auth;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ExpenseControl.Api.Tests.Features.Auth;

public class CognitoAudienceValidatorTests
{
    private const string ExpectedClientId = "test-app-client-id";

    [Fact]
    public void WhenAudClaimMatchesClientId_ReturnsTrue()
    {
        var audiences = new[] { ExpectedClientId };
        var token = CreateJsonWebToken(aud: ExpectedClientId, clientId: null);

        var result = CognitoAuthExtensions.ValidateCognitoAudience(audiences, token, ExpectedClientId);

        Assert.True(result);
    }

    [Fact]
    public void WhenClientIdClaimMatchesAppClientId_ReturnsTrue()
    {
        var token = CreateJsonWebToken(aud: null, clientId: ExpectedClientId);

        var result = CognitoAuthExtensions.ValidateCognitoAudience([], token, ExpectedClientId);

        Assert.True(result);
    }

    [Fact]
    public void WhenNeitherAudNorClientIdMatch_ReturnsFalse()
    {
        var token = CreateJsonWebToken(aud: null, clientId: "wrong-client-id");

        var result = CognitoAuthExtensions.ValidateCognitoAudience([], token, ExpectedClientId);

        Assert.False(result);
    }

    [Fact]
    public void WhenAudiencesNull_FallsBackToClientId()
    {
        var token = CreateJsonWebToken(aud: null, clientId: ExpectedClientId);

        var result = CognitoAuthExtensions.ValidateCognitoAudience(null, token, ExpectedClientId);

        Assert.True(result);
    }

    [Fact]
    public void WhenAudiencesEmpty_FallsBackToClientId()
    {
        var token = CreateJsonWebToken(aud: null, clientId: ExpectedClientId);

        var result = CognitoAuthExtensions.ValidateCognitoAudience([], token, ExpectedClientId);

        Assert.True(result);
    }

    [Fact]
    public void WhenNoClaimsPresent_ReturnsFalse()
    {
        var token = CreateJsonWebToken(aud: null, clientId: null);

        var result = CognitoAuthExtensions.ValidateCognitoAudience(null, token, ExpectedClientId);

        Assert.False(result);
    }

    [Fact]
    public void WhenAudDoesNotMatchButClientIdDoes_ReturnsTrue()
    {
        var audiences = new[] { "wrong-audience" };
        var token = CreateJsonWebToken(aud: "wrong-audience", clientId: ExpectedClientId);

        var result = CognitoAuthExtensions.ValidateCognitoAudience(audiences, token, ExpectedClientId);

        Assert.True(result);
    }

    private static JsonWebToken CreateJsonWebToken(string? aud, string? clientId)
    {
        var claims = new Dictionary<string, object>();

        if (aud is not null)
            claims["aud"] = aud;

        if (clientId is not null)
            claims["client_id"] = clientId;

        // Build a minimal unsigned JWT for testing audience validation
        var descriptor = new SecurityTokenDescriptor
        {
            Claims = claims,
            Issuer = "https://cognito-idp.us-east-1.amazonaws.com/test-pool"
        };

        var handler = new JsonWebTokenHandler();
        var tokenString = handler.CreateToken(descriptor);
        return new JsonWebToken(tokenString);
    }
}
