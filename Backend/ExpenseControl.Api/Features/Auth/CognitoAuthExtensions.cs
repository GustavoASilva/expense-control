using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;

namespace ExpenseControl.Api.Features.Auth;

/// <summary>
/// Extension methods to configure AWS Cognito JWT Bearer authentication.
/// </summary>
public static class CognitoAuthExtensions
{
    /// <summary>
    /// Adds AWS Cognito JWT Bearer authentication and authorization services.
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
                ValidateAudience = true,
                ValidAudience = appClientId,
                ValidateLifetime = true
            };
        });

        services.AddAuthorization();

        return services;
    }
}
