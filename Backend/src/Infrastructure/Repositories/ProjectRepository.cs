using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Interfaces;
using Dapper;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ProjectRepository : GenericRepository<Project>, IProjectRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public ProjectRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public async Task<IEnumerable<Project>> GetProjectsByPortfolioIdAsync(int portfolioId)
        {
            using (var connection = _applicationDbContext.Database.GetDbConnection())
            {
                const string sql = @"
                            SELECT Id, Name, Description, StartDate, EndDate, IsCompleted, PortfolioId, CreatedAt, UpdatedAt
                            FROM Projects
                            WHERE PortfolioId = @portfolioId
                            ORDER BY CreatedAt DESC";
                return await connection.QueryAsync<Project>(sql, new { portfolioId });
            }
        }

        public async Task<Project?> GetProjectWithPortfolioAsync(int projectId)
        {
            using (var connection = _applicationDbContext.Database.GetDbConnection())
            {
                const string sql = @"
                SELECT p.Id, p.Name, p.Description, p.StartDate, p.EndDate, p.IsCompleted, p.PortfolioId, p.CreatedAt, p.UpdatedAt,
                   pf.Id, pf.Name, pf.Description, pf.UserId, pf.CreatedAt, pf.UpdatedAt
                FROM Projects p
                INNER JOIN Portfolios pf ON p.PortfolioId = pf.Id
                WHERE p.Id = @projectId";

                var result = await connection.QueryAsync<Project, Portfolio, Project>(
                    sql,
                    (project, portfolio) =>
                    {
                        project.Portfolio = portfolio;
                        return project;
                    },
                    new { projectId },
                    splitOn: "Id");

                return result.FirstOrDefault();
            }
        }

        public async Task<Project?> GetProjectWithSkillsAsync(int projectId)
        {
            using (var connection = _applicationDbContext.Database.GetDbConnection())
            {
                const string sql = @"
                SELECT p.Id, p.Name, p.Description, p.StartDate, p.EndDate, p.IsCompleted, p.PortfolioId, p.CreatedAt, p.UpdatedAt,
                   ps.ProjectId, ps.SkillId,
                   s.Id, s.Name, s.Category, s.CreatedAt, s.UpdatedAt
                FROM Projects p
                LEFT JOIN ProjectSkills ps ON p.Id = ps.ProjectId
                LEFT JOIN Skills s ON ps.SkillId = s.Id
                WHERE p.Id = @projectId";

                var projectDict = new Dictionary<int, Project>();

                await connection.QueryAsync<Project, ProjectSkill, Skill, Project>(
                    sql,
                    (project, projectSkill, skill) =>
                    {
                        if (!projectDict.TryGetValue(project.Id, out var existingProject))
                        {
                            existingProject = project;
                            existingProject.ProjectSkills = new List<ProjectSkill>();
                            projectDict.Add(project.Id, existingProject);
                        }

                        if (projectSkill != null && skill != null)
                        {
                            projectSkill.Skill = skill;
                            existingProject.ProjectSkills.Add(projectSkill);
                        }

                        return existingProject;
                    },
                    new { projectId },
                    splitOn: "ProjectId,Id");

                return projectDict.Values.FirstOrDefault();
            }
        }

        public async Task<IEnumerable<Project>> GetCompletedProjectsAsync(int portfolioId)
        {
            using (var connection = _applicationDbContext.Database.GetDbConnection())
            {
                const string sql = @"
                SELECT Id, Name, Description, StartDate, EndDate, IsCompleted, PortfolioId, CreatedAt, UpdatedAt
                FROM Projects
                WHERE PortfolioId = @portfolioId AND IsCompleted = 1
                ORDER BY EndDate DESC";

                return await connection.QueryAsync<Project>(sql, new { portfolioId });
            }
        }

        public async Task<IEnumerable<Project>> GetActiveProjectsAsync(int portfolioId)
        {
            using (var connection = _applicationDbContext.Database.GetDbConnection())
            {
                const string sql = @"
                SELECT Id, Name, Description, StartDate, EndDate, IsCompleted, PortfolioId, CreatedAt, UpdatedAt
                FROM Projects
                WHERE PortfolioId = @portfolioId AND IsCompleted = 0
                ORDER BY StartDate DESC";

                return await connection.QueryAsync<Project>(sql, new { portfolioId });
            }
        }
    }
}
