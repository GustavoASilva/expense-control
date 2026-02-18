using System.Security.Claims;
using System.Text.Encodings.Web;
using ExpenseControl.Api.Features.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace ExpenseControl.Api.Tests.Features.Auth;

public class MockAuthenticationHandlerTests
{
    private readonly Mock<IOptionsMonitor<AuthenticationSchemeOptions>> _mockOptions;
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;
    private readonly Mock<ILogger<MockAuthenticationHandler>> _mockLogger;
    private readonly UrlEncoder _urlEncoder;
    private readonly Mock<HttpContext> _mockHttpContext;

    public MockAuthenticationHandlerTests()
    {
        _mockOptions = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();
        _mockLoggerFactory = new Mock<ILoggerFactory>();
        _mockLogger = new Mock<ILogger<MockAuthenticationHandler>>();
        _urlEncoder = UrlEncoder.Default;
        _mockHttpContext = new Mock<HttpContext>();

        _mockOptions
            .Setup(x => x.Get(It.IsAny<string>()))
            .Returns(new AuthenticationSchemeOptions());

        _mockLoggerFactory
            .Setup(x => x.CreateLogger(It.IsAny<string>()))
            .Returns(_mockLogger.Object);

        var mockRequest = new Mock<HttpRequest>();
        _mockHttpContext.Setup(x => x.Request).Returns(mockRequest.Object);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_ReturnsSuccessResult()
    {
        // Arrange
        var handler = new MockAuthenticationHandler(
            _mockOptions.Object,
            _mockLoggerFactory.Object,
            _urlEncoder);

        await InitializeHandler(handler);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Ticket);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_SetsCorrectClaimValues()
    {
        // Arrange
        var handler = new MockAuthenticationHandler(
            _mockOptions.Object,
            _mockLoggerFactory.Object,
            _urlEncoder);

        await InitializeHandler(handler);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.True(result.Succeeded);
        var claims = result.Ticket!.Principal.Claims.ToList();
        
        var nameIdentifierClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        var nameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
        var emailClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

        Assert.NotNull(nameIdentifierClaim);
        Assert.Equal("mock-user-id", nameIdentifierClaim.Value);
        
        Assert.NotNull(nameClaim);
        Assert.Equal("Mock User", nameClaim.Value);
        
        Assert.NotNull(emailClaim);
        Assert.Equal("mock@example.com", emailClaim.Value);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_NameIdentifierMatchesFrontendExpectation()
    {
        // Arrange
        var handler = new MockAuthenticationHandler(
            _mockOptions.Object,
            _mockLoggerFactory.Object,
            _urlEncoder);

        await InitializeHandler(handler);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.True(result.Succeeded);
        var nameIdentifierClaim = result.Ticket!.Principal.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

        // This value must match the userId used in the frontend AuthContext
        Assert.Equal("mock-user-id", nameIdentifierClaim?.Value);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_CreatesIdentityWithCorrectAuthenticationType()
    {
        // Arrange
        var handler = new MockAuthenticationHandler(
            _mockOptions.Object,
            _mockLoggerFactory.Object,
            _urlEncoder);

        await InitializeHandler(handler);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("Mock", result.Ticket!.Principal.Identity?.AuthenticationType);
        Assert.True(result.Ticket.Principal.Identity?.IsAuthenticated);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_SetsCorrectAuthenticationScheme()
    {
        // Arrange
        var handler = new MockAuthenticationHandler(
            _mockOptions.Object,
            _mockLoggerFactory.Object,
            _urlEncoder);

        await InitializeHandler(handler);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("Mock", result.Ticket!.AuthenticationScheme);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_SetsAllRequiredClaims()
    {
        // Arrange
        var handler = new MockAuthenticationHandler(
            _mockOptions.Object,
            _mockLoggerFactory.Object,
            _urlEncoder);

        await InitializeHandler(handler);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.True(result.Succeeded);
        var claims = result.Ticket!.Principal.Claims.ToList();
        
        // Verify all three required claims are present
        Assert.Equal(3, claims.Count);
        Assert.Contains(claims, c => c.Type == ClaimTypes.NameIdentifier);
        Assert.Contains(claims, c => c.Type == ClaimTypes.Name);
        Assert.Contains(claims, c => c.Type == ClaimTypes.Email);
    }

    private async Task InitializeHandler(MockAuthenticationHandler handler)
    {
        var scheme = new AuthenticationScheme("Mock", "Mock", typeof(MockAuthenticationHandler));
        await handler.InitializeAsync(scheme, _mockHttpContext.Object);
    }
}
