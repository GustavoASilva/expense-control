using System.Security.Claims;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Auth;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Households;

public record CreateHouseholdRequest(string Name);
public record JoinHouseholdRequest(Guid InviteId);

public static class HouseholdEndpoints
{
    public static void MapHouseholdEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/households", async (ExpenseDbContext db, CreateHouseholdRequest request, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();

            var household = new Household
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                InviteId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            var member = new HouseholdMember
            {
                Id = Guid.NewGuid(),
                HouseholdId = household.Id,
                UserId = userId,
                Role = "Owner",
                JoinedAt = DateTime.UtcNow
            };

            db.Households.Add(household);
            db.HouseholdMembers.Add(member);
            await db.SaveChangesAsync();

            return Results.Created($"/api/households/{household.Id}", household.ToResponse());
        })
        .WithName("CreateHousehold")
        .WithOpenApi();

        app.MapPost("/api/households/join", async (ExpenseDbContext db, JoinHouseholdRequest request, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();

            // Check if user already belongs to a household
            var existingMembership = await db.HouseholdMembers.AnyAsync(m => m.UserId == userId);
            if (existingMembership)
            {
                return Results.Conflict("You already belong to a household.");
            }

            // Find household by invite ID
            var household = await db.Households.FirstOrDefaultAsync(h => h.InviteId == request.InviteId);
            if (household is null)
            {
                return Results.NotFound("No household found with this invite code.");
            }

            var member = new HouseholdMember
            {
                Id = Guid.NewGuid(),
                HouseholdId = household.Id,
                UserId = userId,
                Role = "Member",
                JoinedAt = DateTime.UtcNow
            };

            db.HouseholdMembers.Add(member);
            await db.SaveChangesAsync();

            return Results.Ok(household.ToResponse());
        })
        .WithName("JoinHousehold")
        .WithOpenApi();

        app.MapGet("/api/households", async (ExpenseDbContext db) =>
        {
            var households = await db.Households
                .OrderBy(h => h.Name)
                .ToListAsync();

            return Results.Ok(households.ToResponse());
        })
        .WithName("ListHouseholds")
        .WithOpenApi();

        app.MapGet("/api/households/{id}", async (ExpenseDbContext db, Guid id) =>
        {
            var household = await db.Households.FindAsync(id);
            return household is null ? Results.NotFound() : Results.Ok(household.ToResponse());
        })
        .WithName("GetHousehold")
        .WithOpenApi();

        app.MapGet("/api/users/me/household", async (ExpenseDbContext db, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();

            var membership = await db.HouseholdMembers
                .Include(m => m.Household)
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (membership is null)
            {
                return Results.Ok(new UserHouseholdResponse(null));
            }

            return Results.Ok(new UserHouseholdResponse(membership.Household.ToResponse()));
        })
        .WithName("GetUserHousehold")
        .WithOpenApi();
    }
}

/// <summary>
/// Response for the user household check endpoint.
/// Household is null when the user does not belong to any household.
/// </summary>
public record UserHouseholdResponse(HouseholdResponse? Household);

