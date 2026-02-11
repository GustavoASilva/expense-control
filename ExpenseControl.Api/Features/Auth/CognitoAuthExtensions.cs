using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace ExpenseControl.Api.Features.Auth;

/// <summary>
/// Extension methods to configure AWS Cognito JWT Bearer authentication.
/// </summary>
public static class CognitoAuthExtensions
{
    /// <summary>
    /// Adds AWS Cognito JWT Bearer authentication and authorization services.
    /// </summary>
    public static IServiceCollection AddCognitoAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var region = configuration["Cognito:Region"];
        var userPoolId = configuration["Cognito:UserPoolId"];
        var appClientId = configuration["Cognito:AppClientId"];

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
                ValidateAudience = true,
                ValidAudience = appClientId,
                ValidateLifetime = true
            };
        });

        services.AddAuthorization();

        return services;
    }
}
