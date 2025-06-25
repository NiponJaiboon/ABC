using API.Extensions;
using Application.Mappings;
using Application.Services;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure HTTPS redirection with proper port
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
    options.HttpsPort = 5001; // Explicitly set HTTPS port
});

// Register services and repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();

// Register application services
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Add controllers
builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Test database connection
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            await context.Database.CanConnectAsync();
            app.Logger.LogInformation("Database connection successful");
            await SeedDatabaseAsync(context);
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Database connection failed: {Message}", ex.Message);
        }
    }
}

// เพิ่ม endpoint ทดสอบง่ายๆ ก่อน
EndpointExtensions.ConfigureHealthCheckEndpoints(app);

app.Run();


static async Task SeedDatabaseAsync(ApplicationDbContext context)
{
    // Seed Skills
    if (!context.Skills.Any())
    {
        var skills = new[]
        {
            new Skill { Name = "C#", Category = "Programming Language" },
            new Skill { Name = "ASP.NET Core", Category = "Framework" },
            new Skill { Name = "Entity Framework", Category = "ORM" },
            new Skill { Name = "PostgreSQL", Category = "Database" },
            new Skill { Name = "React", Category = "Frontend Framework" },
            new Skill { Name = "Next.js", Category = "Frontend Framework" }
        };

        context.Skills.AddRange(skills);
        await context.SaveChangesAsync();
    }
}
