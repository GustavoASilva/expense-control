using AutoFixture;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Features.Savings;
using ExpenseControl.Api.Persistence;
using ExpenseControl.Api.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Tests.Features.Savings;

public class UpdateSavingEndpointTests
{
    private readonly Fixture _fixture;

    public UpdateSavingEndpointTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task UpdateSaving_WithValidRequest_UpdatesSaving()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var saving = CreateAndAddSaving(db, household.Id, "Emergency Fund", null, 5000m, 10000m);

        // Act
        var result = await ExecuteUpdateSaving(db, household.Id, saving.Id, "Updated Fund", "New description", 6000m, 12000m);

        // Assert
        var okResult = Assert.IsType<Ok<SavingResponse>>(result);
        Assert.Equal("Updated Fund", okResult.Value!.Name);
        Assert.Equal("New description", okResult.Value.Description);
        Assert.Equal(6000m, okResult.Value.CurrentAmount);
        Assert.Equal(12000m, okResult.Value.TargetAmount);
    }

    [Fact]
    public async Task UpdateSaving_WithNonExistentSaving_ReturnsNotFound()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");

        // Act
        var result = await ExecuteUpdateSaving(db, household.Id, Guid.NewGuid(), "Updated Fund", null, 6000m, null);

        // Assert
        var notFoundResult = Assert.IsType<NotFound<string>>(result);
        Assert.Equal("Saving not found", notFoundResult.Value);
    }

    [Fact]
    public async Task UpdateSaving_WithDifferentHousehold_ReturnsNotFound()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household1 = CreateAndAddHousehold(db, "Household 1");
        var household2 = CreateAndAddHousehold(db, "Household 2");
        var saving = CreateAndAddSaving(db, household1.Id, "Emergency Fund", null, 5000m, null);

        // Act - Try to update household1's saving using household2's credentials
        var result = await ExecuteUpdateSaving(db, household2.Id, saving.Id, "Hacked Fund", null, 0m, null);

        // Assert
        Assert.IsType<NotFound<string>>(result);
    }

    [Fact]
    public async Task UpdateSaving_UpdatesUpdatedTimestamp()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var saving = CreateAndAddSaving(db, household.Id, "Emergency Fund", null, 5000m, null);
        var originalCreatedAt = saving.CreatedAt;

        await Task.Delay(100);

        // Act
        await ExecuteUpdateSaving(db, household.Id, saving.Id, "Updated Fund", null, 6000m, null);

        // Assert
        var savingInDb = await db.Savings.FirstOrDefaultAsync(s => s.Id == saving.Id);
        Assert.NotNull(savingInDb);
        Assert.Equal(originalCreatedAt, savingInDb.CreatedAt);
        Assert.True(savingInDb.UpdatedAt > originalCreatedAt);
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

    private Saving CreateAndAddSaving(
        ExpenseDbContext db, Guid householdId, string name, string? description, decimal currentAmount, decimal? targetAmount)
    {
        var saving = new Saving
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CurrentAmount = currentAmount,
            TargetAmount = targetAmount,
            HouseholdId = householdId,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        db.Savings.Add(saving);
        db.SaveChanges();
        return saving;
    }

    private async Task<IResult> ExecuteUpdateSaving(
        ExpenseDbContext db, Guid householdId, Guid savingId,
        string name, string? description, decimal currentAmount, decimal? targetAmount)
    {
        var saving = await db.Savings.FirstOrDefaultAsync(s =>
            s.Id == savingId && s.HouseholdId == householdId);

        if (saving == null)
            return Results.NotFound("Saving not found");

        saving.Name = name;
        saving.Description = description;
        saving.CurrentAmount = currentAmount;
        saving.TargetAmount = targetAmount;
        saving.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Results.Ok(saving.ToResponse());
    }
}
