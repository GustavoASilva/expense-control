using Microsoft.EntityFrameworkCore;
using FluentValidation;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using OpenTelemetry;
using System.Diagnostics.Metrics;
using ExpenseControl.Api.Features.Categories;
using ExpenseControl.Api.Features.Transactions.Create;
using ExpenseControl.Api.Features.Transactions.List;
using ExpenseControl.Api.Features.Transactions.Delete;
using ExpenseControl.Api.Features.Transactions.Get;
using ExpenseControl.Api.Features.Balance;

var builder = WebApplication.CreateBuilder(args);

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

// OpenTelemetry Meter and Counter
const string MeterName = "ExpenseControl.Api.Metrics";
builder.Services.AddSingleton<Meter>(_ => new Meter(MeterName));
builder.Services.AddSingleton<Counter<int>>(sp =>
{
    var meter = sp.GetRequiredService<Meter>();
    return meter.CreateCounter<int>("transactions_added", description: "Number of transactions added");
});

// Add OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddMeter(MeterName)
        .AddPrometheusExporter())
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter());

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<ExpenseControl.Api.Persistence.ExpenseDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

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

// Balance endpoints
app.MapGetBalanceEndpoint();
app.MapGetBalanceByCategoryEndpoint();
app.MapGetMonthlyBalanceEndpoint();

// Run EF Core migrations at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ExpenseControl.Api.Persistence.ExpenseDbContext>();
    // var connection = db.Database.GetDbConnection();
    // connection.Open();
    // var anyTables = connection.GetSchema("Tables").Rows.Count > 0;
    // if (!anyTables)
    // {
        db.Database.Migrate();
    // }
    // connection.Close();
}

app.Run();
