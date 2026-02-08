using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Json;
using ExpenseControl.Api.Persistence;
using ExpenseControl.Api.Features.Categories;
using ExpenseControl.Api.Features.Transactions.Create;
using ExpenseControl.Api.Features.Transactions.List;
using ExpenseControl.Api.Features.Transactions.Delete;
using ExpenseControl.Api.Features.Transactions.Get;
using ExpenseControl.Api.Features.Transactions.Update;
using ExpenseControl.Api.Features.Balance;
using ExpenseControl.Api.Features.Budgets.Create;
using ExpenseControl.Api.Features.Budgets.List;
using ExpenseControl.Api.Features.Budgets.Usage;
using ExpenseControl.Api.Features.Households;

var builder = WebApplication.CreateBuilder(args);

// Explicitly set configuration base path and add config files from Configuration/
builder.Configuration.Sources.Clear();
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile(Path.Combine("Configuration", "appsettings.json"), optional: false, reloadOnChange: true)
    .AddJsonFile(Path.Combine("Configuration", $"appsettings.{builder.Environment.EnvironmentName}.json"), optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add health checks
builder.Services.AddHealthChecks();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<ExpenseDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Enable System.Text.Json enum serialization as strings for Minimal APIs
builder.Services.Configure<JsonOptions>(options => options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// Map health check endpoint
app.MapHealthChecks("/health");

// Map API endpoints
app.MapCategoryEndpoints();

// Transaction endpoints
app.MapCreateTransactionEndpoint();
app.MapListTransactionsEndpoint();
app.MapGetTransactionEndpoint();
app.MapDeleteTransactionEndpoint();
app.MapUpdateTransactionEndpoint();

// Balance endpoints
app.MapGetBalanceEndpoint();
app.MapGetBalanceByCategoryEndpoint();
app.MapGetMonthlyBalanceEndpoint();

// Budget endpoints
app.MapCreateBudgetEndpoint();
app.MapListBudgetsEndpoint();
app.MapGetBudgetUsageEndpoint();

// Household endpoints
app.MapHouseholdEndpoints();

// Run EF Core migrations at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ExpenseDbContext>();
    db.Database.Migrate();
}

app.Run();
