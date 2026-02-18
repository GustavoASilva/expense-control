using AutoFixture;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Persistence;
using ExpenseControl.Api.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Tests.Features.Households;

public class HouseholdEndpointsTests
{
    private readonly Fixture _fixture;

    public HouseholdEndpointsTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task CreateHousehold_WithValidRequest_CreatesHousehold()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var request = new { Name = "My Household" };

        // Act
        var result = await ExecuteCreateHousehold(db, request.Name);

        // Assert
        var createdResult = Assert.IsType<Created<Household>>(result);
        Assert.NotNull(createdResult.Value);
        Assert.Equal(request.Name, createdResult.Value.Name);
        Assert.NotEqual(Guid.Empty, createdResult.Value.Id);
        Assert.True(createdResult.Value.CreatedAt <= DateTime.UtcNow);
        Assert.True(createdResult.Value.CreatedAt >= DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task CreateHousehold_SetsCreatedTimestamp()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var beforeCreate = DateTime.UtcNow;

        // Act
        var result = await ExecuteCreateHousehold(db, "Test Household");

        var afterCreate = DateTime.UtcNow;

        // Assert
        var createdResult = Assert.IsType<Created<Household>>(result);
        Assert.True(createdResult.Value!.CreatedAt >= beforeCreate && createdResult.Value.CreatedAt <= afterCreate);
    }

    [Fact]
    public async Task ListHouseholds_ReturnsAllHouseholds()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var households = new List<Household>
        {
            CreateHousehold("Household A"),
            CreateHousehold("Household B"),
            CreateHousehold("Household C")
        };

        db.Households.AddRange(households);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteListHouseholds(db);

        // Assert
        var okResult = Assert.IsType<Ok<List<Household>>>(result);
        Assert.Equal(3, okResult.Value!.Count);
    }

    [Fact]
    public async Task ListHouseholds_OrdersByName()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var households = new List<Household>
        {
            CreateHousehold("Charlie Household"),
            CreateHousehold("Alpha Household"),
            CreateHousehold("Bravo Household")
        };

        db.Households.AddRange(households);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteListHouseholds(db);

        // Assert
        var okResult = Assert.IsType<Ok<List<Household>>>(result);
        Assert.Equal("Alpha Household", okResult.Value![0].Name);
        Assert.Equal("Bravo Household", okResult.Value[1].Name);
        Assert.Equal("Charlie Household", okResult.Value[2].Name);
    }

    [Fact]
    public async Task ListHouseholds_WithNoHouseholds_ReturnsEmptyList()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        // Act
        var result = await ExecuteListHouseholds(db);

        // Assert
        var okResult = Assert.IsType<Ok<List<Household>>>(result);
        Assert.Empty(okResult.Value!);
    }

    [Fact]
    public async Task GetHousehold_WithValidId_ReturnsHousehold()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateHousehold("Test Household");
        db.Households.Add(household);
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteGetHousehold(db, household.Id);

        // Assert
        var okResult = Assert.IsType<Ok<Household>>(result);
        Assert.Equal(household.Id, okResult.Value!.Id);
        Assert.Equal("Test Household", okResult.Value.Name);
    }

    [Fact]
    public async Task GetHousehold_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        // Act
        var result = await ExecuteGetHousehold(db, Guid.NewGuid());

        // Assert
        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task CreateHousehold_PersistsToDatabase()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var householdName = "Persisted Household";

        // Act
        var result = await ExecuteCreateHousehold(db, householdName);

        // Assert
        var createdResult = Assert.IsType<Created<Household>>(result);
        var householdInDb = await db.Households.FindAsync(createdResult.Value!.Id);
        Assert.NotNull(householdInDb);
        Assert.Equal(householdName, householdInDb.Name);
    }

    private Household CreateHousehold(string name)
    {
        return _fixture.Build<Household>()
            .With(h => h.Id, Guid.NewGuid())
            .With(h => h.Name, name)
            .With(h => h.CreatedAt, DateTime.UtcNow)
            .Create();
    }

    private async Task<IResult> ExecuteCreateHousehold(ExpenseDbContext db, string name)
    {
        var household = new Household
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        db.Households.Add(household);
        await db.SaveChangesAsync();

        return Results.Created($"/api/households/{household.Id}", household);
    }

    private async Task<IResult> ExecuteListHouseholds(ExpenseDbContext db)
    {
        var households = await db.Households
            .OrderBy(h => h.Name)
            .ToListAsync();

        return Results.Ok(households);
    }

    private async Task<IResult> ExecuteGetHousehold(ExpenseDbContext db, Guid id)
    {
        var household = await db.Households.FindAsync(id);
        return household is null ? Results.NotFound() : Results.Ok(household);
    }
}
