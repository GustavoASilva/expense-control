using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Auth.Services;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Auth;

public record GoogleLoginRequest(string IdToken);

public record AuthResponse(string Token, UserDto User);

public record UserDto(Guid Id, string Email, string Name, string? PictureUrl, IList<string> Roles);

public static class GoogleAuthEndpoint
{
    public static void MapGoogleAuthEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/google", async (
            GoogleLoginRequest request,
            IGoogleTokenValidator tokenValidator,
            IJwtTokenGenerator tokenGenerator,
            ExpenseDbContext db,
            CancellationToken cancellationToken) =>
        {
            // Validate Google token
            var googleUserInfo = await tokenValidator.ValidateTokenAsync(request.IdToken, cancellationToken);

            if (googleUserInfo == null)
            {
                return Results.Unauthorized();
            }

            // Find or create user
            var user = await db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.GoogleId == googleUserInfo.GoogleId, cancellationToken);

            if (user == null)
            {
                // Create new user
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

                // Assign "Member" role
                var memberRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == WellKnownRoles.Member, cancellationToken);
                if (memberRole == null)
                {
                    throw new InvalidOperationException("Default 'Member' role not found in database. Please ensure migrations have been run.");
                }

                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = memberRole.Id
                };
                db.Set<UserRole>().Add(userRole);

                await db.SaveChangesAsync(cancellationToken);

                // Reload user with roles
                user = await db.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstAsync(u => u.Id == user.Id, cancellationToken);
            }
            else
            {
                // Update existing user info
                user.Email = googleUserInfo.Email;
                user.Name = googleUserInfo.Name;
                user.PictureUrl = googleUserInfo.PictureUrl;
                user.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync(cancellationToken);
            }

            // Generate JWT
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            var token = tokenGenerator.GenerateToken(user, roles);

            var userDto = new UserDto(user.Id, user.Email, user.Name, user.PictureUrl, roles);
            return Results.Ok(new AuthResponse(token, userDto));
        })
        .AllowAnonymous()
        .WithName("GoogleLogin")
        .WithOpenApi();
    }
}
