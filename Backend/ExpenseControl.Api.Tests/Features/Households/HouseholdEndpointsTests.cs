using AutoFixture;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Auth;
using ExpenseControl.Api.Features.Households;
using ExpenseControl.Api.Persistence;
using ExpenseControl.Api.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ExpenseControl.Api.Tests.Features.Households;

public class HouseholdEndpointsTests
{
    private readonly Fixture _fixture;

    public HouseholdEndpointsTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task CreateHousehold_WithValidRequest_CreatesHouseholdAndMember()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var userId = "test-user-id";
        var request = new CreateHouseholdRequest("My Household");

        var result = await ExecuteCreateHousehold(db, request.Name, userId);

        var createdResult = Assert.IsType<Created<HouseholdResponse>>(result);
        Assert.NotNull(createdResult.Value);
        Assert.Equal(request.Name, createdResult.Value.Name);
        Assert.NotEqual(Guid.Empty, createdResult.Value.Id);
        Assert.NotEqual(Guid.Empty, createdResult.Value.InviteId);
        Assert.True(createdResult.Value.CreatedAt <= DateTime.UtcNow);
        Assert.True(createdResult.Value.CreatedAt >= DateTime.UtcNow.AddMinutes(-1));

        var member = await db.HouseholdMembers.FirstOrDefaultAsync(m => m.UserId == userId);
        Assert.NotNull(member);
        Assert.Equal(createdResult.Value.Id, member.HouseholdId);
        Assert.Equal("Owner", member.Role);
    }

    [Fact]
    public async Task CreateHousehold_SetsCreatedTimestamp()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var beforeCreate = DateTime.UtcNow;

        var result = await ExecuteCreateHousehold(db, "Test Household", "test-user");

        var afterCreate = DateTime.UtcNow;

        var createdResult = Assert.IsType<Created<HouseholdResponse>>(result);
        Assert.True(createdResult.Value!.CreatedAt >= beforeCreate && createdResult.Value.CreatedAt <= afterCreate);
    }

    [Fact]
    public async Task ListHouseholds_ReturnsAllHouseholds()
    {
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

        var result = await ExecuteListHouseholds(db);

        var okResult = Assert.IsType<Ok<List<HouseholdResponse>>>(result);
        Assert.Equal(3, okResult.Value!.Count);
    }

    [Fact]
    public async Task ListHouseholds_OrdersByName()
    {
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

        var result = await ExecuteListHouseholds(db);

        var okResult = Assert.IsType<Ok<List<HouseholdResponse>>>(result);
        Assert.Equal("Alpha Household", okResult.Value![0].Name);
        Assert.Equal("Bravo Household", okResult.Value[1].Name);
        Assert.Equal("Charlie Household", okResult.Value[2].Name);
    }

    [Fact]
    public async Task ListHouseholds_WithNoHouseholds_ReturnsEmptyList()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var result = await ExecuteListHouseholds(db);

        var okResult = Assert.IsType<Ok<List<HouseholdResponse>>>(result);
        Assert.Empty(okResult.Value!);
    }

    [Fact]
    public async Task GetHousehold_WithValidId_ReturnsHousehold()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateHousehold("Test Household");
        db.Households.Add(household);
        await db.SaveChangesAsync();

        var result = await ExecuteGetHousehold(db, household.Id);

        var okResult = Assert.IsType<Ok<HouseholdResponse>>(result);
        Assert.Equal(household.Id, okResult.Value!.Id);
        Assert.Equal("Test Household", okResult.Value.Name);
    }

    [Fact]
    public async Task GetHousehold_WithNonExistentId_ReturnsNotFound()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var result = await ExecuteGetHousehold(db, Guid.NewGuid());

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task CreateHousehold_PersistsToDatabase()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var householdName = "Persisted Household";

        var result = await ExecuteCreateHousehold(db, householdName, "test-user");

        var createdResult = Assert.IsType<Created<HouseholdResponse>>(result);
        var householdInDb = await db.Households.FindAsync(createdResult.Value!.Id);
        Assert.NotNull(householdInDb);
        Assert.Equal(householdName, householdInDb.Name);
    }

    [Fact]
    public async Task GetUserHousehold_WithMembership_ReturnsHousehold()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var userId = "user-with-household";
        var household = CreateHousehold("User's Household");
        db.Households.Add(household);
        db.HouseholdMembers.Add(new HouseholdMember
        {
            Id = Guid.NewGuid(),
            HouseholdId = household.Id,
            UserId = userId,
            Role = "Owner",
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var result = await ExecuteGetUserHousehold(db, userId);

        var okResult = Assert.IsType<Ok<UserHouseholdResponse>>(result);
        Assert.NotNull(okResult.Value!.Household);
        Assert.Equal(household.Id, okResult.Value.Household.Id);
        Assert.Equal("User's Household", okResult.Value.Household.Name);
    }

    [Fact]
    public async Task GetUserHousehold_WithoutMembership_ReturnsNullHousehold()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var result = await ExecuteGetUserHousehold(db, "user-without-household");

        var okResult = Assert.IsType<Ok<UserHouseholdResponse>>(result);
        Assert.Null(okResult.Value!.Household);
    }

    [Fact]
    public async Task CreateHousehold_GeneratesUniqueInviteId()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var result1 = await ExecuteCreateHousehold(db, "Household 1", "user-1");
        var result2 = await ExecuteCreateHousehold(db, "Household 2", "user-2");

        var created1 = Assert.IsType<Created<HouseholdResponse>>(result1);
        var created2 = Assert.IsType<Created<HouseholdResponse>>(result2);
        Assert.NotEqual(created1.Value!.InviteId, created2.Value!.InviteId);
    }

    private Household CreateHousehold(string name)
    {
        return _fixture.Build<Household>()
            .With(h => h.Id, Guid.NewGuid())
            .With(h => h.Name, name)
            .With(h => h.InviteId, Guid.NewGuid())
            .With(h => h.CreatedAt, DateTime.UtcNow)
            .Create();
    }

    private async Task<IResult> ExecuteCreateHousehold(ExpenseDbContext db, string name, string userId)
    {
        var household = new Household
        {
            Id = Guid.NewGuid(),
            Name = name,
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
    }

    private async Task<IResult> ExecuteListHouseholds(ExpenseDbContext db)
    {
        var households = await db.Households
            .OrderBy(h => h.Name)
            .ToListAsync();

        return Results.Ok(households.ToResponse());
    }

    private async Task<IResult> ExecuteGetHousehold(ExpenseDbContext db, Guid id)
    {
        var household = await db.Households.FindAsync(id);
        return household is null ? Results.NotFound() : Results.Ok(household.ToResponse());
    }

    private async Task<IResult> ExecuteGetUserHousehold(ExpenseDbContext db, string userId)
    {
        var membership = await db.HouseholdMembers
            .Include(m => m.Household)
            .FirstOrDefaultAsync(m => m.UserId == userId);

        if (membership is null)
        {
            return Results.Ok(new UserHouseholdResponse(null));
        }

        return Results.Ok(new UserHouseholdResponse(membership.Household.ToResponse()));
    }
}
