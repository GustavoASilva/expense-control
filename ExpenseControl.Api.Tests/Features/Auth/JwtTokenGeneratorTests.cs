using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Auth.Services;
using Microsoft.Extensions.Configuration;

namespace ExpenseControl.Api.Tests.Features.Auth;

public class JwtTokenGeneratorTests
{
    private const string TestSecret = "test-jwt-secret-key-that-is-at-least-32-characters-long";
    private const string TestIssuer = "TestIssuer";
    private const string TestAudience = "TestAudience";

    private static IJwtTokenGenerator CreateGenerator(
        string secret = TestSecret,
        string issuer = TestIssuer,
        string audience = TestAudience,
        string expirationInHours = "24")
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Jwt:Secret"] = secret,
                ["Authentication:Jwt:Issuer"] = issuer,
                ["Authentication:Jwt:Audience"] = audience,
                ["Authentication:Jwt:ExpirationInHours"] = expirationInHours,
            })
            .Build();

        return new JwtTokenGenerator(configuration);
    }

    [Fact]
    public void GenerateToken_WithValidUser_ReturnsValidJwt()
    {
        // Arrange
        var generator = CreateGenerator();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            GoogleId = "google-123"
        };
        var roles = new List<string> { "Member" };

        // Act
        var token = generator.GenerateToken(user, roles);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal(user.Id.ToString(), jwt.Subject);
        Assert.Equal(user.Email, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(user.Name, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Name).Value);
        Assert.Equal(TestIssuer, jwt.Issuer);
        Assert.Contains(TestAudience, jwt.Audiences);
    }

    [Fact]
    public void GenerateToken_WithRoles_IncludesRoleClaims()
    {
        // Arrange
        var generator = CreateGenerator();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@example.com",
            Name = "Admin User",
            GoogleId = "google-456"
        };
        var roles = new List<string> { "Admin", "Member" };

        // Act
        var token = generator.GenerateToken(user, roles);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var roleClaims = jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        Assert.Contains("Admin", roleClaims);
        Assert.Contains("Member", roleClaims);
    }

    [Fact]
    public void GenerateToken_WithPictureUrl_IncludesPictureClaim()
    {
        // Arrange
        var generator = CreateGenerator();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            GoogleId = "google-789",
            PictureUrl = "https://example.com/photo.jpg"
        };
        var roles = new List<string> { "Member" };

        // Act
        var token = generator.GenerateToken(user, roles);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var pictureClaim = jwt.Claims.FirstOrDefault(c => c.Type == "picture");
        Assert.NotNull(pictureClaim);
        Assert.Equal("https://example.com/photo.jpg", pictureClaim.Value);
    }

    [Fact]
    public void GenerateToken_WithNullPictureUrl_DoesNotIncludePictureClaim()
    {
        // Arrange
        var generator = CreateGenerator();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            GoogleId = "google-101",
            PictureUrl = null
        };
        var roles = new List<string> { "Member" };

        // Act
        var token = generator.GenerateToken(user, roles);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var pictureClaim = jwt.Claims.FirstOrDefault(c => c.Type == "picture");
        Assert.Null(pictureClaim);
    }

    [Fact]
    public void GenerateToken_WithMissingSecret_ThrowsInvalidOperationException()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
        var generator = new JwtTokenGenerator(configuration);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            GoogleId = "google-102"
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => generator.GenerateToken(user, new List<string>()));
    }

    [Fact]
    public void GenerateToken_WithCustomExpiration_SetsCorrectExpiry()
    {
        // Arrange
        var generator = CreateGenerator(expirationInHours: "48");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Name = "Test User",
            GoogleId = "google-103"
        };

        // Act
        var token = generator.GenerateToken(user, new List<string> { "Member" });

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var expectedExpiry = DateTime.UtcNow.AddHours(48);
        Assert.True(jwt.ValidTo > DateTime.UtcNow.AddHours(47));
        Assert.True(jwt.ValidTo < expectedExpiry.AddMinutes(1));
    }
}
