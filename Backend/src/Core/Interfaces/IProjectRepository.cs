using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;

namespace Core.Interfaces
{
    public interface IProjectRepository : IGenericRepository<Project>
    {
        Task<IEnumerable<Project>> GetProjectsByPortfolioIdAsync(int portfolioId);
        Task<Project> GetProjectWithPortfolioAsync(int projectId);
        Task<Project> GetProjectWithSkillsAsync(int projectId);
        Task<IEnumerable<Project>> GetCompletedProjectsAsync(int portfolioId);
        Task<IEnumerable<Project>> GetActiveProjectsAsync(int portfolioId);
    }
}
