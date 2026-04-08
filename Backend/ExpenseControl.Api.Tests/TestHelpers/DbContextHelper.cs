using ExpenseControl.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseControl.Api.Tests.TestHelpers;

public static class DbContextHelper
{
    public static ExpenseDbContext CreateInMemoryDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ExpenseDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        return new ExpenseDbContext(options, new NullEncryptionService());
    }
}
