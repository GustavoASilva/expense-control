using System.Security.Claims;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Auth;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Categories;

public record CreateCategoryRequest(string Name, string? Description, TransactionType Type, string? IconName);

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/categories", async (ExpenseDbContext db, ClaimsPrincipal user, TransactionType? type) =>
        {
            var householdId = user.TryGetHouseholdId();

            var query = db.Categories.AsQueryable();

            // Return default (global) categories plus household-specific ones
            query = query.Where(c => c.HouseholdId == null || c.HouseholdId == householdId);

            if (type.HasValue)
            {
                query = query.Where(c => c.Type == type.Value);
            }

            var categories = await query.OrderBy(c => c.Name).ToListAsync();
            return Results.Ok(categories.ToResponse());
        })
        .WithName("GetCategories")
        .WithOpenApi();

        app.MapGet("/api/categories/{id}", async (ExpenseDbContext db, Guid id) =>
        {
            var category = await db.Categories.FindAsync(id);
            return category is null ? Results.NotFound() : Results.Ok(category.ToResponse());
        })
        .WithName("GetCategoryById")
        .WithOpenApi();

        app.MapPost("/api/categories", async (ExpenseDbContext db, CreateCategoryRequest request, ClaimsPrincipal user) =>
        {
            var householdId = user.GetHouseholdId();

            if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length < 2)
            {
                return Results.BadRequest("Category name must be at least 2 characters.");
            }

            if (request.Name.Trim().Length > 100)
            {
                return Results.BadRequest("Category name must be at most 100 characters.");
            }

            // Check for duplicate name within the same type and household (including defaults)
            var duplicate = await db.Categories.AnyAsync(c =>
                c.Name == request.Name.Trim() &&
                c.Type == request.Type &&
                (c.HouseholdId == null || c.HouseholdId == householdId));

            if (duplicate)
            {
                return Results.Conflict("A category with this name and type already exists.");
            }

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                Type = request.Type,
                IconName = string.IsNullOrWhiteSpace(request.IconName) ? "tag" : request.IconName.Trim(),
                HouseholdId = householdId
            };

            db.Categories.Add(category);
            await db.SaveChangesAsync();

            return Results.Created($"/api/categories/{category.Id}", category.ToResponse());
        })
        .WithName("CreateCategory")
        .WithOpenApi();
    }
}
