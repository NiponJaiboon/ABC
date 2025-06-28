using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Infrastructure.Data;

namespace API.Extensions;

public static class SeedDataExtensions
{
    public static async Task SeedDatabaseAsync(ApplicationDbContext context)
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

        // Portfolio seeding is intentionally commented out as it requires valid user IDs
        // Portfolios will be created when users register through the API

        // Seed Projects (disabled as they require portfolios)
        // Projects will be created through the API endpoints after users create portfolios
    }
}
