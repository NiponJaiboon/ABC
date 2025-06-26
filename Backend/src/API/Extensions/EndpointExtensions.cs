using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class EndpointExtensions
    {
        public static WebApplication ConfigureHealthCheckEndpoints(this WebApplication app)
        {
            app.MapGet("/", () => "API is running!")
             .WithName("Root")
            .WithOpenApi()
            .WithTags("General");

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
            .WithOpenApi()
            .WithTags("General");

            return app;
        }
    }
}
