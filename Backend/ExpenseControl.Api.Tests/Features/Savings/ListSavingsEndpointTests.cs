using AutoFixture;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Savings;
using ExpenseControl.Api.Persistence;
using ExpenseControl.Api.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Tests.Features.Savings;

public class ListSavingsEndpointTests
{
    private readonly Fixture _fixture;

    public ListSavingsEndpointTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task ListSavings_ReturnsAllHouseholdSavings()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");

        db.Savings.AddRange(
            CreateSaving(household.Id, "Emergency Fund", 5000m),
            CreateSaving(household.Id, "Vacation Fund", 1000m)
        );
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteListSavings(db, household.Id);

        // Assert
        var okResult = Assert.IsType<Ok<List<SavingResponse>>>(result);
        Assert.Equal(2, okResult.Value!.Count);
    }

    [Fact]
    public async Task ListSavings_DoesNotReturnOtherHouseholdSavings()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household1 = CreateAndAddHousehold(db, "Household 1");
        var household2 = CreateAndAddHousehold(db, "Household 2");

        db.Savings.Add(CreateSaving(household1.Id, "Emergency Fund", 5000m));
        db.Savings.Add(CreateSaving(household2.Id, "Vacation Fund", 1000m));
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteListSavings(db, household1.Id);

        // Assert
        var okResult = Assert.IsType<Ok<List<SavingResponse>>>(result);
        Assert.Single(okResult.Value!);
        Assert.Equal("Emergency Fund", okResult.Value[0].Name);
    }

    [Fact]
    public async Task ListSavings_WithNoSavings_ReturnsEmptyList()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");

        // Act
        var result = await ExecuteListSavings(db, household.Id);

        // Assert
        var okResult = Assert.IsType<Ok<List<SavingResponse>>>(result);
        Assert.Empty(okResult.Value!);
    }

    [Fact]
    public async Task ListSavings_ReturnsSavingsOrderedByName()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");

        db.Savings.AddRange(
            CreateSaving(household.Id, "Vacation Fund", 1000m),
            CreateSaving(household.Id, "Emergency Fund", 5000m)
        );
        await db.SaveChangesAsync();

        // Act
        var result = await ExecuteListSavings(db, household.Id);

        // Assert
        var okResult = Assert.IsType<Ok<List<SavingResponse>>>(result);
        Assert.Equal("Emergency Fund", okResult.Value![0].Name);
        Assert.Equal("Vacation Fund", okResult.Value[1].Name);
    }

    private Household CreateAndAddHousehold(ExpenseDbContext db, string name)
    {
        var household = _fixture.Build<Household>()
            .With(h => h.Id, Guid.NewGuid())
            .With(h => h.Name, name)
            .Create();
        db.Households.Add(household);
        db.SaveChanges();
        return household;
    }

    private Saving CreateSaving(Guid householdId, string name, decimal currentAmount)
    {
        return new Saving
        {
            Id = Guid.NewGuid(),
            Name = name,
            CurrentAmount = currentAmount,
            HouseholdId = householdId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private async Task<IResult> ExecuteListSavings(ExpenseDbContext db, Guid householdId)
    {
        var savings = await db.Savings
            .Where(s => s.HouseholdId == householdId)
            .OrderBy(s => s.Name)
            .ToListAsync();
        return Results.Ok(savings.ToResponse());
    }
}
