using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Households
{
    public record CreateHouseholdRequest(string Name);

    public static class HouseholdEndpoints
    {
        public static void MapHouseholdEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/households", async (ExpenseDbContext db, CreateHouseholdRequest request) =>
            {
                var household = new Household
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    CreatedAt = DateTime.UtcNow
                };

                db.Households.Add(household);
                await db.SaveChangesAsync();

                return Results.Created($"/api/households/{household.Id}", household);
            })
            .WithName("CreateHousehold")
            .WithOpenApi();

            app.MapGet("/api/households", async (ExpenseDbContext db) =>
            {
                var households = await db.Households
                    .OrderBy(h => h.Name)
                    .ToListAsync();

                return Results.Ok(households);
            })
            .WithName("ListHouseholds")
            .WithOpenApi();

            app.MapGet("/api/households/{id}", async (ExpenseDbContext db, Guid id) =>
            {
                var household = await db.Households.FindAsync(id);
                return household is null ? Results.NotFound() : Results.Ok(household);
            })
            .WithName("GetHousehold")
            .WithOpenApi();
        }
    }
}
