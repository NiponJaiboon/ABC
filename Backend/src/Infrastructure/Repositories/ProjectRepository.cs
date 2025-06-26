using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Interfaces;
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
            return await _applicationDbContext.Projects
                .Where(p => p.PortfolioId == portfolioId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Project?> GetProjectWithPortfolioAsync(int projectId)
        {
            return await _applicationDbContext.Projects
                .Include(p => p.Portfolio)
                .FirstOrDefaultAsync(p => p.Id == projectId);
        }

        public async Task<Project?> GetProjectWithSkillsAsync(int projectId)
        {
            return await _applicationDbContext.Projects
                .Include(p => p.ProjectSkills)
                    .ThenInclude(ps => ps.Skill)
                .FirstOrDefaultAsync(p => p.Id == projectId);
        }

        public async Task<IEnumerable<Project>> GetCompletedProjectsAsync(int portfolioId)
        {
            return await _applicationDbContext.Projects
                .Where(p => p.PortfolioId == portfolioId && p.IsCompleted)
                .OrderByDescending(p => p.EndDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetActiveProjectsAsync(int portfolioId)
        {
            return await _applicationDbContext.Projects
                .Where(p => p.PortfolioId == portfolioId && !p.IsCompleted)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }
    }
}
