using AutoFixture;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Categories;
using ExpenseControl.Api.Persistence;
using ExpenseControl.Api.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Tests.Features.Categories;

public class CategoryEndpointsTests
{
    private readonly Fixture _fixture;

    public CategoryEndpointsTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task GetCategories_WithNoFilter_ReturnsAllCategories()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var categories = new List<Category>
        {
            CreateCategory("Food", TransactionType.Expense),
            CreateCategory("Transport", TransactionType.Expense),
            CreateCategory("Salary", TransactionType.Income)
        };

        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteGetCategories(db, null);

        // Assert
        var okResult = Assert.IsType<Ok<List<CategoryResponse>>>(result);
        Assert.Equal(3, okResult.Value!.Count);
    }

    [Fact]
    public async Task GetCategories_FilterByType_ReturnsMatchingCategories()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var categories = new List<Category>
        {
            CreateCategory("Food", TransactionType.Expense),
            CreateCategory("Transport", TransactionType.Expense),
            CreateCategory("Salary", TransactionType.Income),
            CreateCategory("Bonus", TransactionType.Income)
        };

        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteGetCategories(db, TransactionType.Expense);

        // Assert
        var okResult = Assert.IsType<Ok<List<CategoryResponse>>>(result);
        Assert.Equal(2, okResult.Value!.Count);
        Assert.All(okResult.Value, c => Assert.Equal(TransactionType.Expense, c.Type));
    }

    [Fact]
    public async Task GetCategories_FilterByIncome_ReturnsIncomeCategories()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var categories = new List<Category>
        {
            CreateCategory("Food", TransactionType.Expense),
            CreateCategory("Salary", TransactionType.Income),
            CreateCategory("Bonus", TransactionType.Income)
        };

        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteGetCategories(db, TransactionType.Income);

        // Assert
        var okResult = Assert.IsType<Ok<List<CategoryResponse>>>(result);
        Assert.Equal(2, okResult.Value!.Count);
        Assert.All(okResult.Value, c => Assert.Equal(TransactionType.Income, c.Type));
    }

    [Fact]
    public async Task GetCategoryById_WithValidId_ReturnsCategory()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var category = CreateCategory("Food", TransactionType.Expense);
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteGetCategoryById(db, category.Id);

        // Assert
        var okResult = Assert.IsType<Ok<CategoryResponse>>(result);
        Assert.Equal(category.Id, okResult.Value!.Id);
        Assert.Equal("Food", okResult.Value.Name);
    }

    [Fact]
    public async Task GetCategoryById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        // Act
        var result = await ExecuteGetCategoryById(db, Guid.NewGuid());

        // Assert
        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task GetCategoryById_ReturnsAllProperties()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var category = _fixture.Build<Category>()
            .With(c => c.Id, Guid.NewGuid())
            .With(c => c.Name, "Food")
            .With(c => c.Description, "Food and groceries")
            .With(c => c.Type, TransactionType.Expense)
            .With(c => c.IconName, "food_icon")
            .Create();

        db.Categories.Add(category);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteGetCategoryById(db, category.Id);

        // Assert
        var okResult = Assert.IsType<Ok<CategoryResponse>>(result);
        Assert.Equal("Food", okResult.Value!.Name);
        Assert.Equal("Food and groceries", okResult.Value.Description);
        Assert.Equal("food_icon", okResult.Value.IconName);
        Assert.Equal(TransactionType.Expense, okResult.Value.Type);
    }

    private Category CreateCategory(string name, TransactionType type)
    {
        return _fixture.Build<Category>()
            .With(c => c.Id, Guid.NewGuid())
            .With(c => c.Name, name)
            .With(c => c.Type, type)
            .Create();
    }

    private async Task<IResult> ExecuteGetCategories(ExpenseDbContext db, TransactionType? type)
    {
        var query = db.Categories.AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(c => c.Type == type.Value);
        }

        var categories = await query.ToListAsync();
        return Results.Ok(categories.ToResponse());
    }

    private async Task<IResult> ExecuteGetCategoryById(ExpenseDbContext db, Guid id)
    {
        var category = await db.Categories.FindAsync(id);
        return category is null ? Results.NotFound() : Results.Ok(category.ToResponse());
    }
}
