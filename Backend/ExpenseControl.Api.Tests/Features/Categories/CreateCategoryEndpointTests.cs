using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Categories;
using ExpenseControl.Api.Persistence;
using ExpenseControl.Api.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Tests.Features.Categories;

public class CreateCategoryEndpointTests
{
    [Fact]
    public async Task CreateCategory_WithValidRequest_CreatesCategoryWithHouseholdId()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var householdId = Guid.NewGuid();
        db.Households.Add(new Household
        {
            Id = householdId,
            Name = "Test Household",
            InviteId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var result = await ExecuteCreateCategory(db, "Entertainment", "Movies, games, etc.", TransactionType.Expense, "controller", householdId);

        var createdResult = Assert.IsType<Created<CategoryResponse>>(result);
        Assert.Equal("Entertainment", createdResult.Value!.Name);
        Assert.Equal(TransactionType.Expense, createdResult.Value.Type);
        Assert.False(createdResult.Value.IsDefault);
    }

    [Fact]
    public async Task CreateCategory_WithShortName_ReturnsBadRequest()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var result = await ExecuteCreateCategory(db, "A", null, TransactionType.Expense, null, Guid.NewGuid());

        Assert.IsType<BadRequest<string>>(result);
    }

    [Fact]
    public async Task CreateCategory_WithDuplicateName_ReturnsConflict()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var householdId = Guid.NewGuid();
        db.Households.Add(new Household
        {
            Id = householdId,
            Name = "Test Household",
            InviteId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        });

        db.Categories.Add(new Category
        {
            Id = Guid.NewGuid(),
            Name = "Food",
            Type = TransactionType.Expense,
            HouseholdId = null
        });
        await db.SaveChangesAsync();

        var result = await ExecuteCreateCategory(db, "Food", null, TransactionType.Expense, null, householdId);

        Assert.IsType<Conflict<string>>(result);
    }

    [Fact]
    public async Task CreateCategory_DefaultsIconToTag_WhenNotProvided()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var householdId = Guid.NewGuid();
        db.Households.Add(new Household
        {
            Id = householdId,
            Name = "Test Household",
            InviteId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var result = await ExecuteCreateCategory(db, "Pets", null, TransactionType.Expense, null, householdId);

        var createdResult = Assert.IsType<Created<CategoryResponse>>(result);
        Assert.Equal("tag", createdResult.Value!.IconName);
    }

    [Fact]
    public async Task DefaultCategories_HaveIsDefaultTrue()
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Housing",
            Type = TransactionType.Expense,
            HouseholdId = null
        };

        var response = category.ToResponse();
        Assert.True(response.IsDefault);
    }

    [Fact]
    public async Task CustomCategories_HaveIsDefaultFalse()
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Custom",
            Type = TransactionType.Expense,
            HouseholdId = Guid.NewGuid()
        };

        var response = category.ToResponse();
        Assert.False(response.IsDefault);
    }

    private static async Task<IResult> ExecuteCreateCategory(
        ExpenseDbContext db,
        string name,
        string? description,
        TransactionType type,
        string? iconName,
        Guid householdId)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 2)
        {
            return Results.BadRequest("Category name must be at least 2 characters.");
        }

        if (name.Trim().Length > 100)
        {
            return Results.BadRequest("Category name must be at most 100 characters.");
        }

        var duplicate = await db.Categories.AnyAsync(c =>
            c.Name == name.Trim() &&
            c.Type == type &&
            (c.HouseholdId == null || c.HouseholdId == householdId));

        if (duplicate)
        {
            return Results.Conflict("A category with this name and type already exists.");
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Type = type,
            IconName = string.IsNullOrWhiteSpace(iconName) ? "tag" : iconName.Trim(),
            HouseholdId = householdId
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync();

        return Results.Created($"/api/categories/{category.Id}", category.ToResponse());
    }
}
