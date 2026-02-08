using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ExpenseControl.Api.Persistence;

public class ExpenseDbContextFactory : IDesignTimeDbContextFactory<ExpenseDbContext>
{
    public ExpenseDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables();

        var configuration = builder.Build();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<ExpenseDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new ExpenseDbContext(optionsBuilder.Options);
    }
}
