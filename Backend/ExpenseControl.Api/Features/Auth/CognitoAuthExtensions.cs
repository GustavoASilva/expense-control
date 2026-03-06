using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;

namespace ExpenseControl.Api.Features.Auth;

/// <summary>
/// Extension methods to configure AWS Cognito JWT Bearer authentication.
/// </summary>
public static class CognitoAuthExtensions
{
    /// <summary>
    /// The claim type used by Cognito access tokens to identify the app client.
    /// Access tokens carry <c>client_id</c> instead of the standard <c>aud</c> claim.
    /// </summary>
    public const string CognitoClientIdClaimType = "client_id";

    /// <summary>
    /// Adds AWS Cognito JWT Bearer authentication and authorization services.
    /// Registers <see cref="HouseholdClaimsTransformation"/> to resolve the
    /// household identifier from the database after authentication.
    /// </summary>
    public static IServiceCollection AddCognitoAuthentication(this IServiceCollection services, WebApplicationBuilder builder)
    {
        var authType = builder.Configuration["Authentication:Type"];
        if (authType == "Mock")
        {
            if (!builder.Environment.IsDevelopment())
            {
                throw new InvalidOperationException("Mock authentication cannot be used in non-development environments");
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Mock";
                options.DefaultChallengeScheme = "Mock";
            })
            .AddScheme<AuthenticationSchemeOptions, MockAuthenticationHandler>("Mock", options => { });

            services.AddAuthorization();
            return services;
        }

        var region = builder.Configuration["Cognito:Region"];
        var userPoolId = builder.Configuration["Cognito:UserPoolId"];
        var appClientId = builder.Configuration["Cognito:AppClientId"];

        ArgumentException.ThrowIfNullOrWhiteSpace(region, nameof(region));
        ArgumentException.ThrowIfNullOrWhiteSpace(userPoolId, nameof(userPoolId));
        ArgumentException.ThrowIfNullOrWhiteSpace(appClientId, nameof(appClientId));

        var authority = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = authority;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = authority,
                ValidateLifetime = true,
                // Cognito access tokens carry 'client_id' instead of the standard 'aud' claim.
                // A custom validator accepts both ID tokens (aud) and access tokens (client_id).
                ValidateAudience = true,
                AudienceValidator = (audiences, securityToken, _) =>
                    ValidateCognitoAudience(audiences, securityToken, appClientId)
            };
        });

        // Resolve householdId from the database for Cognito-authenticated users
        services.AddScoped<IClaimsTransformation, HouseholdClaimsTransformation>();

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Validates the audience for both Cognito ID tokens (standard <c>aud</c> claim)
    /// and access tokens (<c>client_id</c> claim).
    /// </summary>
    public static bool ValidateCognitoAudience(
        IEnumerable<string>? audiences,
        SecurityToken securityToken,
        string expectedClientId)
    {
        // Standard 'aud' claim — present in Cognito ID tokens
        if (audiences?.Contains(expectedClientId) == true)
            return true;

        // Cognito access tokens use 'client_id' instead of 'aud'
        if (securityToken is JsonWebToken jwt
            && jwt.TryGetClaim(CognitoClientIdClaimType, out var clientIdClaim)
            && clientIdClaim.Value == expectedClientId)
        {
            return true;
        }

        return false;
    }
}
