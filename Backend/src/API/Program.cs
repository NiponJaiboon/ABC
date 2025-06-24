using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// เพิ่ม endpoint ทดสอบง่ายๆ ก่อน
app.MapGet("/", () => "API is running!")
    .WithName("Root")
    .WithOpenApi();

app.MapGet("/test", () => new
{
    Message = "Test endpoint working",
    Time = DateTime.UtcNow
})
    .WithName("Test")
    .WithOpenApi();

app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Time = DateTime.UtcNow
}))
    .WithName("Health")
    .WithOpenApi();

// Database Health Check Endpoint
app.MapGet("/health/db", async (ApplicationDbContext context) =>
{
    try
    {
        // ทดสอบการเชื่อมต่อ
        await context.Database.CanConnectAsync();

        // ดึงข้อมูลเวอร์ชันของ PostgreSQL
        var version = await context.Database.ExecuteSqlRawAsync("SELECT version()");

        return Results.Ok(new
        {
            Console = "Database Health Check",
            Status = "Connected",
            Database = "PostgreSQL",
            Timestamp = DateTime.UtcNow,
            Message = "Successfully connected to Supabase"
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

// Existing WeatherForecast endpoint
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
