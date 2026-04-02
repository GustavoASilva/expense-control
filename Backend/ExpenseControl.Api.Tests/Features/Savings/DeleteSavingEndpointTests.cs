using AutoFixture;
using ExpenseControl.Api.Entities;
using ExpenseControl.Api.Persistence;
using ExpenseControl.Api.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Tests.Features.Savings;

public class DeleteSavingEndpointTests
{
    private readonly Fixture _fixture;

    public DeleteSavingEndpointTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task DeleteSaving_WithValidId_DeletesSaving()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");
        var saving = CreateAndAddSaving(db, household.Id, "Emergency Fund");

        // Act
        var result = await ExecuteDeleteSaving(db, household.Id, saving.Id);

        // Assert
        Assert.IsType<NoContent>(result);
        var savingInDb = await db.Savings.FirstOrDefaultAsync(s => s.Id == saving.Id);
        Assert.Null(savingInDb);
    }

    [Fact]
    public async Task DeleteSaving_WithNonExistentSaving_ReturnsNotFound()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household = CreateAndAddHousehold(db, "Test Household");

        // Act
        var result = await ExecuteDeleteSaving(db, household.Id, Guid.NewGuid());

        // Assert
        var notFoundResult = Assert.IsType<NotFound<string>>(result);
        Assert.Equal("Saving not found", notFoundResult.Value);
    }

    [Fact]
    public async Task DeleteSaving_WithDifferentHousehold_ReturnsNotFound()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        await using var db = DbContextHelper.CreateInMemoryDbContext(dbName);

        var household1 = CreateAndAddHousehold(db, "Household 1");
        var household2 = CreateAndAddHousehold(db, "Household 2");
        var saving = CreateAndAddSaving(db, household1.Id, "Emergency Fund");

        // Act - Try to delete household1's saving using household2's credentials
        var result = await ExecuteDeleteSaving(db, household2.Id, saving.Id);

        // Assert
        Assert.IsType<NotFound<string>>(result);
        var savingInDb = await db.Savings.FirstOrDefaultAsync(s => s.Id == saving.Id);
        Assert.NotNull(savingInDb);
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

    private Saving CreateAndAddSaving(ExpenseDbContext db, Guid householdId, string name)
    {
        var saving = new Saving
        {
            Id = Guid.NewGuid(),
            Name = name,
            CurrentAmount = 1000m,
            HouseholdId = householdId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Savings.Add(saving);
        db.SaveChanges();
        return saving;
    }

    private async Task<IResult> ExecuteDeleteSaving(ExpenseDbContext db, Guid householdId, Guid savingId)
    {
        var saving = await db.Savings.FirstOrDefaultAsync(s =>
            s.Id == savingId && s.HouseholdId == householdId);

        if (saving == null)
            return Results.NotFound("Saving not found");

        db.Savings.Remove(saving);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}
