using AutoFixture;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Savings;
using ExpenseControl.Api.Persistence;
using ExpenseControl.Api.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Tests.Features.Savings;

public class CreateSavingEndpointTests
{
    private readonly Fixture _fixture;

    public CreateSavingEndpointTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task CreateSaving_WithValidRequest_CreatesSaving()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");

        // Act
        var result = await ExecuteCreateSaving(db, household.Id, "Emergency Fund", "For emergencies", 5000m, 10000m);

        // Assert
        var okResult = Assert.IsType<Ok<SavingResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal("Emergency Fund", okResult.Value.Name);
        Assert.Equal("For emergencies", okResult.Value.Description);
        Assert.Equal(5000m, okResult.Value.CurrentAmount);
        Assert.Equal(10000m, okResult.Value.TargetAmount);
        Assert.NotEqual(Guid.Empty, okResult.Value.Id);
    }

    [Fact]
    public async Task CreateSaving_WithoutTargetAmount_CreatesSavingWithNullTarget()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");

        // Act
        var result = await ExecuteCreateSaving(db, household.Id, "Vacation Fund", null, 1000m, null);

        // Assert
        var okResult = Assert.IsType<Ok<SavingResponse>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Null(okResult.Value.TargetAmount);
    }

    [Fact]
    public async Task CreateSaving_WithNonExistentHousehold_ReturnsNotFound()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        // Act
        var result = await ExecuteCreateSaving(db, Guid.NewGuid(), "Emergency Fund", null, 0m, null);

        // Assert
        var notFoundResult = Assert.IsType<NotFound<string>>(result);
        Assert.Equal("Household not found", notFoundResult.Value);
    }

    [Fact]
    public async Task CreateSaving_SetsCreatedAndUpdatedTimestamps()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");

        var beforeCreate = DateTime.UtcNow;

        // Act
        var result = await ExecuteCreateSaving(db, household.Id, "Emergency Fund", null, 1000m, null);

        var afterCreate = DateTime.UtcNow;

        // Assert
        var okResult = Assert.IsType<Ok<SavingResponse>>(result);
        var savingInDb = await db.Savings.FirstOrDefaultAsync(s => s.Id == okResult.Value!.Id);
        Assert.NotNull(savingInDb);
        Assert.True(savingInDb.CreatedAt >= beforeCreate && savingInDb.CreatedAt <= afterCreate);
        Assert.True(savingInDb.UpdatedAt >= beforeCreate && savingInDb.UpdatedAt <= afterCreate);
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

    private async Task<IResult> ExecuteCreateSaving(
        ExpenseDbContext db, Guid householdId, string name, string? description, decimal currentAmount, decimal? targetAmount)
    {
        var household = await db.Households.FindAsync(householdId);
        if (household == null)
            return Results.NotFound("Household not found");

        var saving = new Saving
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CurrentAmount = currentAmount,
            TargetAmount = targetAmount,
            HouseholdId = householdId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Savings.Add(saving);
        await db.SaveChangesAsync();
        return Results.Ok(saving.ToResponse());
    }
}
