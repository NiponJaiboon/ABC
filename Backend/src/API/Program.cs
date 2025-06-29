using API.Extensions;
using API.Middleware;
using Application.Mappings;
using Application.Services;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.ConfigureSerilog();

    Log.Information("Starting ABC API...");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Configure Identity & Authentication
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    // Add JWT Authentication and Authorization
    builder.Services.AddAllAuthenticationServices(builder.Configuration);

    // Step 13: Add Security Middleware (Rate Limiting, CORS, Security Headers)
    builder.Services.AddSecurityMiddleware(builder.Configuration);

    // Step 14: Add Audit & Logging Services
    builder.Services.AddScoped<IAuthenticationAuditService, EnhancedAuthenticationAuditService>();
    builder.Services.AddScoped<IFailedLoginTrackingService, FailedLoginTrackingService>();
    builder.Services.AddScoped<IUserActivityAuditService, UserActivityAuditService>();
    builder.Services.AddScoped<ISecurityAuditService, SecurityAuditService>();
    builder.Services.AddScoped<CompositeAuditService>();

    // Keep legacy service for backward compatibility
    builder.Services.AddScoped<ILegacyAuthenticationAuditService, LegacyAuthenticationAuditService>();

    // Add HSTS for production
    builder.Services.AddHsts(options =>
    {
        options.Preload = true;
        options.IncludeSubDomains = true;
        options.MaxAge = TimeSpan.FromDays(365);
    });

    // Configure HTTPS redirection with proper port
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
        options.HttpsPort = 5001; // Explicitly set HTTPS port
    });

    // Register services and repositories
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();
    builder.Services.AddScoped<IProjectRepository, ProjectRepository>();

    // Register Generic Repositories for Skills
    builder.Services.AddScoped<IGenericRepository<Skill>, GenericRepository<Skill>>();
    builder.Services.AddScoped<IGenericRepository<ProjectSkill>, GenericRepository<ProjectSkill>>();

    // Register application services
    builder.Services.AddScoped<IPortfolioService, PortfolioService>();
    builder.Services.AddScoped<IProjectService, ProjectService>();
    builder.Services.AddScoped<ISkillService, SkillService>();
    builder.Services.AddScoped<IProjectSkillService, ProjectSkillService>();

    // Register authorization data seeder
    builder.Services.AddScoped<AuthorizationDataSeeder>();

    // Register AutoMapper
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

                // Seed data
                await DataSeeder.SeedRolesAsync(scope.ServiceProvider);
                await DataSeeder.SeedAdminUserAsync(scope.ServiceProvider);

                SeedDataExtensions.SeedDatabaseAsync(context).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Database connection failed: {Message}", ex.Message);
            }
        }
    }

    // Use HTTPS redirection only in production
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    // Step 13: Apply Security Middleware (Rate Limiting, CORS, Security Headers)
    app.UseSecurityMiddleware();

    // Additional Security Headers (HSTS)
    app.UseHsts();

    // Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    EndpointExtensions.ConfigureHealthCheckEndpoints(app);

    app.MapControllers();

    // Seed authorization data in development
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<AuthorizationDataSeeder>();
        await seeder.SeedDefaultDataAsync();
    }

    Log.Information("ABC API started successfully");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
