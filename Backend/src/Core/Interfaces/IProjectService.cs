using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;

#nullable enable

namespace Core.Interfaces
{
    public interface IProjectService
    {
        Task<IEnumerable<Project>> GetAllProjectsAsync();
        Task<Project?> GetProjectByIdAsync(int id);
        Task<IEnumerable<Project>> GetProjectsByPortfolioIdAsync(int portfolioId);
        Task<Project> CreateProjectAsync(Project project);
        Task<Project> UpdateProjectAsync(Project project);
        Task<bool> DeleteProjectAsync(int id);
        Task<bool> ProjectExistsAsync(int id);
        Task<bool> UserOwnsProjectAsync(string userId, int projectId);

    }
}
