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
    public class PortfolioRepository : IPortfolioRepository
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public PortfolioRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;

        }
        public async Task<IEnumerable<Portfolio>> GetPortfolioByUserAsync(string userId)
        {
            using (var connection = _applicationDbContext.Database.GetDbConnection())
            {
                var sql = "SELECT * FROM Portfolios WHERE UserId = @UserId";
                var portfolios = await connection.QueryAsync<Portfolio>(sql, new { UserId = userId });
                return portfolios;
            }
        }

        public async Task<Portfolio?> GetPortfolioWithDetailsAsync(int portfolioId)
        {
            using (var connection = _applicationDbContext.Database.GetDbConnection())
            {
                var sql = "SELECT * FROM Portfolios WHERE Id = @Id";
                var portfolio = await connection.QueryFirstOrDefaultAsync<Portfolio>(
                    sql,
                    new { Id = portfolioId }
                );
                return portfolio;
            }
        }

        public async Task<Portfolio?> GetPortfolioWithProjectsAsync(int id)
        {
            using (var connection = _applicationDbContext.Database.GetDbConnection())
            {
            // Get portfolio
            var portfolioSql = "SELECT * FROM Portfolios WHERE Id = @Id";
            var portfolio = await connection.QueryFirstOrDefaultAsync<Portfolio>(portfolioSql, new { Id = id });
            
            if (portfolio == null)
                return null;

            // Get projects for the portfolio
            var projectsSql = "SELECT * FROM Projects WHERE PortfolioId = @PortfolioId";
            var projects = await connection.QueryAsync<Project>(projectsSql, new { PortfolioId = id });
            
            portfolio.Projects = projects.ToList();

            // Get skills for each project
            foreach (var project in portfolio.Projects)
            {
                var skillsSql = @"
                SELECT ps.*, s.* 
                FROM ProjectSkills ps 
                INNER JOIN Skills s ON ps.SkillId = s.Id 
                WHERE ps.ProjectId = @ProjectId";
                
                var projectSkills = await connection.QueryAsync<ProjectSkill, Skill, ProjectSkill>(
                skillsSql,
                (projectSkill, skill) =>
                {
                    projectSkill.Skill = skill;
                    return projectSkill;
                },
                new { ProjectId = project.Id },
                splitOn: "Id"
                );
                
                project.ProjectSkills = projectSkills.ToList();
            }

            return portfolio;
            }
        }
    }
}
