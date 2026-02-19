using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Features.Categories;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/categories", async (ExpenseDbContext db, TransactionType? type) =>
        {
            var query = db.Categories.AsQueryable();

            if (type.HasValue)
            {
                query = query.Where(c => c.Type == type.Value);
            }

            var categories = await query.ToListAsync();
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
    }
}
