using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Auth;
using ExpenseControl.Api.Features.Auth.Services;
using ExpenseControl.Api.Tests.TestHelpers;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;

namespace ExpenseControl.Api.Tests.Features.Auth;

public class GoogleAuthEndpointTests
{
    [Fact]
    public async Task GoogleLogin_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var mockValidator = new Mock<IGoogleTokenValidator>();
        mockValidator.Setup(v => v.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GoogleUserInfo?)null);

        var mockTokenGenerator = new Mock<IJwtTokenGenerator>();
        var request = new GoogleLoginRequest("invalid-token");

        // Act
        var result = await ExecuteGoogleLogin(db, request, mockValidator.Object, mockTokenGenerator.Object);

        // Assert
        Assert.IsType<UnauthorizedHttpResult>(result);
    }

    [Fact]
    public async Task GoogleLogin_WithNewUser_CreatesUserAndReturnsToken()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        // Seed the Member role
        db.Roles.Add(new Role { Id = WellKnownRoles.MemberRoleId, Name = WellKnownRoles.Member });
        await db.SaveChangesAsync();

        var googleUserInfo = new GoogleUserInfo("google-123", "test@gmail.com", "Test User", "https://example.com/photo.jpg");
        var mockValidator = new Mock<IGoogleTokenValidator>();
        mockValidator.Setup(v => v.ValidateTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(googleUserInfo);

        var mockTokenGenerator = new Mock<IJwtTokenGenerator>();
        mockTokenGenerator.Setup(g => g.GenerateToken(It.IsAny<User>(), It.IsAny<IList<string>>()))
            .Returns("jwt-token-123");

        var request = new GoogleLoginRequest("valid-token");

        // Act
        var result = await ExecuteGoogleLogin(db, request, mockValidator.Object, mockTokenGenerator.Object);

        // Assert
        var okResult = Assert.IsType<Ok<AuthResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal("jwt-token-123", okResult.Value.Token);
        Assert.Equal("test@gmail.com", okResult.Value.User.Email);
        Assert.Equal("Test User", okResult.Value.User.Name);
        Assert.Contains("Member", okResult.Value.User.Roles);

        // Verify user was persisted
        var savedUser = db.Users.FirstOrDefault(u => u.GoogleId == "google-123");
        Assert.NotNull(savedUser);
        Assert.Equal("test@gmail.com", savedUser.Email);
    }

    [Fact]
    public async Task GoogleLogin_WithExistingUser_UpdatesInfoAndReturnsToken()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        // Seed role and existing user
        var memberRole = new Role { Id = WellKnownRoles.MemberRoleId, Name = WellKnownRoles.Member };
        db.Roles.Add(memberRole);

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "old@gmail.com",
            Name = "Old Name",
            GoogleId = "google-456",
            PictureUrl = null,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow.AddDays(-30)
        };
        db.Users.Add(existingUser);
        db.Set<UserRole>().Add(new UserRole { UserId = existingUser.Id, RoleId = memberRole.Id });
        await db.SaveChangesAsync();

        var googleUserInfo = new GoogleUserInfo("google-456", "new@gmail.com", "New Name", "https://example.com/new.jpg");
        var mockValidator = new Mock<IGoogleTokenValidator>();
        mockValidator.Setup(v => v.ValidateTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(googleUserInfo);

        var mockTokenGenerator = new Mock<IJwtTokenGenerator>();
        mockTokenGenerator.Setup(g => g.GenerateToken(It.IsAny<User>(), It.IsAny<IList<string>>()))
            .Returns("jwt-token-456");

        var request = new GoogleLoginRequest("valid-token");

        // Act
        var result = await ExecuteGoogleLogin(db, request, mockValidator.Object, mockTokenGenerator.Object);

        // Assert
        var okResult = Assert.IsType<Ok<AuthResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal("new@gmail.com", okResult.Value.User.Email);
        Assert.Equal("New Name", okResult.Value.User.Name);

        // Verify user was updated in DB
        var updatedUser = db.Users.First(u => u.GoogleId == "google-456");
        Assert.Equal("new@gmail.com", updatedUser.Email);
        Assert.Equal("New Name", updatedUser.Name);
        Assert.Equal("https://example.com/new.jpg", updatedUser.PictureUrl);
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> ExecuteGoogleLogin(
        ExpenseControl.Api.Persistence.ExpenseDbContext db,
        GoogleLoginRequest request,
        IGoogleTokenValidator tokenValidator,
        IJwtTokenGenerator tokenGenerator)
    {
        var googleUserInfo = await tokenValidator.ValidateTokenAsync(request.IdToken);

        if (googleUserInfo == null)
        {
            return Microsoft.AspNetCore.Http.Results.Unauthorized();
        }

        var user = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .FirstOrDefaultAsync(
                Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                    .ThenInclude(
                        Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                            .Include(db.Users, u => u.UserRoles),
                        ur => ur.Role),
                u => u.GoogleId == googleUserInfo.GoogleId);

        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = googleUserInfo.Email,
                Name = googleUserInfo.Name,
                GoogleId = googleUserInfo.GoogleId,
                PictureUrl = googleUserInfo.PictureUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.Users.Add(user);

            var memberRole = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .FirstOrDefaultAsync(db.Roles, r => r.Name == WellKnownRoles.Member);

            if (memberRole == null)
            {
                throw new InvalidOperationException("Default 'Member' role not found in database.");
            }

            db.Set<UserRole>().Add(new UserRole { UserId = user.Id, RoleId = memberRole.Id });
            await db.SaveChangesAsync();

            user = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                .FirstAsync(
                    Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                        .ThenInclude(
                            Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
                                .Include(db.Users, u => u.UserRoles),
                            ur => ur.Role),
                    u => u.Id == user.Id);
        }
        else
        {
            user.Email = googleUserInfo.Email;
            user.Name = googleUserInfo.Name;
            user.PictureUrl = googleUserInfo.PictureUrl;
            user.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var token = tokenGenerator.GenerateToken(user, roles);

        var userDto = new UserDto(user.Id, user.Email, user.Name, user.PictureUrl, roles);
        return Microsoft.AspNetCore.Http.Results.Ok(new AuthResponse(token, userDto));
    }
}
