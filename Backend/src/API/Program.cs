using Application.Mappings;
using Infrastructure.Data;
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

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

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
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Database connection failed: {Message}", ex.Message);
        }
    }
}

// เพิ่ม endpoint ทดสอบง่ายๆ ก่อน
app.MapGet("/", () => "API is running!")
    .WithName("Root")
    .WithOpenApi();

// Database Health Check Endpoint
app.MapGet("/health/db", async (ApplicationDbContext context) =>
{
    try
    {
        // ทดสอบการเชื่อมต่อ
        await context.Database.CanConnectAsync();

        // ดึงข้อมูลเวอร์ชันของ PostgreSQL
        var connectionString = context.Database.GetConnectionString();

        return Results.Ok(new
        {
            Console = "Database Health Check",
            Status = "Connected",
            Database = "PostgreSQL",
            Timestamp = DateTime.UtcNow,
            Message = "Successfully connected to Supabase",
            ConnectionInfo = new
            {
                Host = connectionString?.Contains("Host=") == true ? connectionString.Split("Host=")[1].Split(";")[0] : "N/A",
                Database = connectionString?.Contains("Database=") == true ? connectionString.Split("Database=")[1].Split(";")[0] : "N/A"
            }
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Database Connection Error",
            type: "https://httpstatuses.org/500"
        );
    }
})
.WithName("DatabaseHealthCheck")
.WithOpenApi();

app.Run();
