#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProjectRepository _projectRepository;
        private readonly IPortfolioService _portfolioService;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(
            IUnitOfWork unitOfWork,
            IProjectRepository projectRepository,
            IPortfolioService portfolioService,
            ILogger<ProjectService> logger)
        {
            _unitOfWork = unitOfWork;
            _projectRepository = projectRepository;
            _portfolioService = portfolioService;
            _logger = logger;
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all projects");
                return await _unitOfWork.Repository<Project>().GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all projects");
                throw;
            }
        }

        public async Task<Project?> GetProjectByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving project with ID: {ProjectId}", id);
                return await _unitOfWork.Repository<Project>().GetByIdAsync(id);
            }
            catch (KeyNotFoundException)
            {
                _logger.LogInformation("Project with ID {ProjectId} not found", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project with ID: {ProjectId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Project>> GetProjectsByPortfolioIdAsync(int portfolioId)
        {
            try
            {
                _logger.LogInformation("Retrieving projects for portfolio: {PortfolioId}", portfolioId);
                return await _projectRepository.GetProjectsByPortfolioIdAsync(portfolioId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving projects for portfolio: {PortfolioId}", portfolioId);
                throw;
            }
        }

        public async Task<Project> CreateProjectAsync(Project project)
        {
            try
            {
                _logger.LogInformation("Creating new project: {ProjectTitle} for Portfolio: {PortfolioId}",
                    project.Title, project.PortfolioId);

                // Validate portfolio exists
                if (!await _portfolioService.PortfolioExistsAsync(project.PortfolioId))
                {
                    throw new ArgumentException($"Portfolio with ID {project.PortfolioId} does not exist");
                }

                // Validate project data
                if (string.IsNullOrWhiteSpace(project.Title))
                {
                    throw new ArgumentException("Project title is required");
                }

                // Set timestamps
                project.CreatedAt = DateTime.UtcNow;

                // If no start date provided, use current date
                if (project.StartDate == default)
                {
                    project.StartDate = DateTime.UtcNow;
                }

                await _unitOfWork.Repository<Project>().AddAsync(project);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Project created successfully with ID: {ProjectId}", project.Id);
                return project;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                throw;
            }
        }

        public async Task<Project> UpdateProjectAsync(Project project)
        {
            try
            {
                _logger.LogInformation("Updating project with ID: {ProjectId}", project.Id);

                // Check if project exists
                var existingProject = await GetProjectByIdAsync(project.Id);
                if (existingProject == null)
                {
                    throw new ArgumentException($"Project with ID {project.Id} does not exist");
                }

                project.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Repository<Project>().UpdateAsync(project);
                await _unitOfWork.CommitAsync();

                return project;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project with ID: {ProjectId}", project.Id);
                throw;
            }
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting project with ID: {ProjectId}", id);

                var project = await GetProjectByIdAsync(id);
                if (project == null)
                    return false;

                await _unitOfWork.Repository<Project>().DeleteAsync(id);
                await _unitOfWork.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project with ID: {ProjectId}", id);
                throw;
            }
        }

        public async Task<bool> ProjectExistsAsync(int id)
        {
            try
            {
                var project = await GetProjectByIdAsync(id);
                return project != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if project exists: {ProjectId}", id);
                throw;
            }
        }

        public async Task<bool> UserOwnsProjectAsync(string userId, int projectId)
        {
            try
            {
                var project = await _projectRepository.GetProjectWithPortfolioAsync(projectId);
                return project?.Portfolio?.UserId == userId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking project ownership: UserId={UserId}, ProjectId={ProjectId}",
                    userId, projectId);
                throw;
            }
        }
    }
}
