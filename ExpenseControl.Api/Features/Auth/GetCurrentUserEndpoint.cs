using System.Security.Claims;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Auth;

public static class GetCurrentUserEndpoint
{
    public static void MapGetCurrentUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/auth/me", async (
            ClaimsPrincipal user,
            ExpenseDbContext db,
            CancellationToken cancellationToken) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            var dbUser = await db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (dbUser == null)
            {
                return Results.NotFound();
            }

            var roles = dbUser.UserRoles.Select(ur => ur.Role.Name).ToList();
            var userDto = new UserDto(dbUser.Id, dbUser.Email, dbUser.Name, dbUser.PictureUrl, roles);

            return Results.Ok(userDto);
        })
        .RequireAuthorization()
        .WithName("GetCurrentUser")
        .WithOpenApi();
    }
}
