using API.Extensions;
using Application.Mappings;
using Application.Services;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.ConfigureSerilog();

    Log.Information("Starting ABC API...");

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
                Log.Information("Database connection successful");
                SeedDataExtensions.SeedDatabaseAsync(context).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Database connection failed: {Message}", ex.Message);
            }
        }
    }

    EndpointExtensions.ConfigureHealthCheckEndpoints(app);

    app.MapControllers();

    Log.Information("ABC API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}



