using Microsoft.EntityFrameworkCore;
using RoutingV3.Engine;
using RoutingV3.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// EF Core with SQLite
builder.Services.AddDbContext<RoutingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=routing.db"));

// Routing engine services
builder.Services.AddSingleton<PostalCodeMatcher>();
builder.Services.AddSingleton<GraphBuilder>();
builder.Services.AddSingleton<CostCalculator>();
builder.Services.AddSingleton<EtaCalculator>();
builder.Services.AddSingleton<PathFinder>();
builder.Services.AddTransient<RoutingEngine>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RoutingDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
