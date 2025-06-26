using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Infrastructure.Data;

namespace API.Extensions
{
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

            // Seed Portfolios
            if (!context.Portfolios.Any())
            {
                var portfolios = new[]
                {
                    new Portfolio { Title = "Portfolio 1", Description = "Description for Portfolio 1", CreatedAt = DateTime.UtcNow },
                    new Portfolio { Title = "Portfolio 2", Description = "Description for Portfolio 2", CreatedAt = DateTime.UtcNow }
                };

                context.Portfolios.AddRange(portfolios);
                await context.SaveChangesAsync();
            }

            // Seed Projects
            if (!context.Projects.Any())
            {
                var projects = new[]
                {
                    new Project { Title = "Project 1", Description = "Description for Project 1", CreatedAt = DateTime.UtcNow },
                    new Project { Title = "Project 2", Description = "Description for Project 2", CreatedAt = DateTime.UtcNow }
                };

                context.Projects.AddRange(projects);
                await context.SaveChangesAsync();
            }
        }
    }
}