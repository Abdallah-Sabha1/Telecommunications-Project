using Aithos.Infrastructure.Persistence;
using Aithos.Infrastructure.Persistence.Repositories;
using Aithos.Infrastructure.AI;
using Aithos.Infrastructure.Reporting;
using Aithos.Infrastructure.Imports;
using Aithos.Application.Features;
using Aithos.Application.Abstractions.Persistence;
using Aithos.Application.Abstractions.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5000");

builder.Services.AddControllers();

// Cache the generated carbon report for repeated dashboard requests.
builder.Services.AddMemoryCache();

builder.Services.AddDbContext<AithosDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories contain all Entity Framework Core queries.
builder.Services.AddScoped<ITowerRepository, TowerRepository>();
builder.Services.AddScoped<IReadingRepository, ReadingRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<ITowerAnalysisRepository, TowerAnalysisRepository>();
builder.Services.AddScoped<IMonthlyPriceRepository, MonthlyPriceRepository>();
builder.Services.AddScoped<IDatasetRepository, DatasetRepository>();
builder.Services.AddScoped<IAiAnalysisResultRepository, AiAnalysisResultRepository>();

// Services contain the application and business rules.
builder.Services.AddScoped<ITowerService, TowerService>();
builder.Services.AddScoped<IReadingService, ReadingService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IAnalysisService, AnalysisService>();
builder.Services.AddScoped<ICarbonIntelligenceService, CarbonIntelligenceService>();
builder.Services.AddScoped<IEnergyAnalyticsService, EnergyAnalyticsService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IDatasetImportService, DatasetImportService>();
builder.Services.AddScoped<IAiImportService, AiImportService>();

// External AI adapter for tower analysis.
builder.Services.AddHttpClient<IClaudeService, ClaudeService>(client =>
{
    client.BaseAddress = new Uri("https://api.anthropic.com/");
    client.Timeout = TimeSpan.FromSeconds(60);
});

builder.Services.AddCors(options =>
    options.AddPolicy("frontend", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AithosDbContext>();
    await db.Database.MigrateAsync();
}

app.UseCors("frontend");
app.MapControllers();
app.Run();

