using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Dtos;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IPortfolioService _portfolioService;
        private readonly IMapper _mapper;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(
            IProjectService projectService,
            IPortfolioService portfolioService,
            IMapper mapper,
            ILogger<ProjectController> logger)
        {
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _portfolioService = portfolioService ?? throw new ArgumentNullException(nameof(portfolioService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all projects
        /// </summary>
        /// <returns>List of all projects</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProjectDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetAllProjects()
        {
            try
            {
                _logger.LogInformation("Retrieving all projects");
                var projects = await _projectService.GetAllProjectsAsync();
                var projectDtos = _mapper.Map<IEnumerable<ProjectDto>>(projects);

                _logger.LogInformation("Retrieved {Count} projects", projectDtos.Count());
                return Ok(projectDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all projects");
                return StatusCode(500, new
                {
                    Error = "Internal server error",
                    Message = "An error occurred while retrieving projects"
                });
            }
        }

        /// <summary>
        /// Get project by ID
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>Project details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProjectDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ProjectDto>> GetProjectById(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving project with ID: {ProjectId}", id);

                if (id <= 0)
                {
                    return BadRequest("Project ID must be greater than 0");
                }

                var project = await _projectService.GetProjectByIdAsync(id);

                if (project == null)
                {
                    _logger.LogWarning("Project with ID {ProjectId} not found", id);
                    return NotFound(new { Message = $"Project with ID {id} not found" });
                }

                var projectDto = _mapper.Map<ProjectDto>(project);
                return Ok(projectDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving project with ID: {ProjectId}", id);
                return StatusCode(500, new
                {
                    Error = "Internal server error",
                    Message = "An error occurred while retrieving the project"
                });
            }
        }

        /// <summary>
        /// Get projects by portfolio ID
        /// </summary>
        /// <param name="portfolioId">Portfolio ID</param>
        /// <returns>List of projects in the portfolio</returns>
        [HttpGet("portfolio/{portfolioId}")]
        [ProducesResponseType(typeof(IEnumerable<ProjectDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjectsByPortfolioId(int portfolioId)
        {
            try
            {
                _logger.LogInformation("Retrieving projects for portfolio: {PortfolioId}", portfolioId);

                if (portfolioId <= 0)
                {
                    return BadRequest("Portfolio ID must be greater than 0");
                }

                // Check if portfolio exists
                if (!await _portfolioService.PortfolioExistsAsync(portfolioId))
                {
                    _logger.LogWarning("Portfolio with ID {PortfolioId} not found", portfolioId);
                    return NotFound(new { Message = $"Portfolio with ID {portfolioId} not found" });
                }

                var projects = await _projectService.GetProjectsByPortfolioIdAsync(portfolioId);
                var projectDtos = _mapper.Map<IEnumerable<ProjectDto>>(projects);

                _logger.LogInformation("Retrieved {Count} projects for portfolio {PortfolioId}",
                    projectDtos.Count(), portfolioId);

                return Ok(projectDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving projects for portfolio: {PortfolioId}", portfolioId);
                return StatusCode(500, new
                {
                    Error = "Internal server error",
                    Message = "An error occurred while retrieving projects"
                });
            }
        }

        /// <summary>
        /// Create new project in portfolio
        /// </summary>
        /// <param name="request">Project creation data</param>
        /// <returns>Created project</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ProjectDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectRequest request)
        {
            try
            {
                _logger.LogInformation("Creating new project: {ProjectTitle} for Portfolio: {PortfolioId}",
                    request.Title, request.PortfolioId);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if portfolio exists
                if (!await _portfolioService.PortfolioExistsAsync(request.PortfolioId))
                {
                    _logger.LogWarning("Portfolio with ID {PortfolioId} does not exist", request.PortfolioId);
                    return BadRequest(new { Message = $"Portfolio with ID {request.PortfolioId} does not exist" });
                }

                // Map DTO to Entity using AutoMapper
                var project = _mapper.Map<Project>(request);

                var createdProject = await _projectService.CreateProjectAsync(project);
                var projectDto = _mapper.Map<ProjectDto>(createdProject);

                _logger.LogInformation("Project created successfully with ID: {ProjectId}", createdProject.Id);

                return CreatedAtAction(
                    nameof(GetProjectById),
                    new { id = createdProject.Id },
                    projectDto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid project data provided");
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                return StatusCode(500, new
                {
                    Error = "Internal server error",
                    Message = "An error occurred while creating the project"
                });
            }
        }

        /// <summary>
        /// Update existing project
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="request">Project update data</param>
        /// <returns>Updated project</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProjectDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ProjectDto>> UpdateProject(int id, [FromBody] UpdateProjectRequest request)
        {
            try
            {
                _logger.LogInformation("Updating project with ID: {ProjectId}", id);

                if (id <= 0)
                {
                    return BadRequest("Project ID must be greater than 0");
                }

                if (id != request.Id)
                {
                    return BadRequest(new { Message = "Project ID in URL does not match ID in request body" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if project exists
                if (!await _projectService.ProjectExistsAsync(id))
                {
                    _logger.LogWarning("Project with ID {ProjectId} not found for update", id);
                    return NotFound(new { Message = $"Project with ID {id} not found" });
                }

                // Map DTO to Entity using AutoMapper
                var project = _mapper.Map<Project>(request);

                var updatedProject = await _projectService.UpdateProjectAsync(project);
                var projectDto = _mapper.Map<ProjectDto>(updatedProject);

                _logger.LogInformation("Project with ID {ProjectId} updated successfully", id);
                return Ok(projectDto);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid project data provided for update");
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating project with ID: {ProjectId}", id);
                return StatusCode(500, new
                {
                    Error = "Internal server error",
                    Message = "An error occurred while updating the project"
                });
            }
        }

        /// <summary>
        /// Delete project
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> DeleteProject(int id)
        {
            try
            {
                _logger.LogInformation("Deleting project with ID: {ProjectId}", id);

                if (id <= 0)
                {
                    return BadRequest("Project ID must be greater than 0");
                }

                var deleted = await _projectService.DeleteProjectAsync(id);

                if (!deleted)
                {
                    _logger.LogWarning("Project with ID {ProjectId} not found for deletion", id);
                    return NotFound(new { Message = $"Project with ID {id} not found" });
                }

                _logger.LogInformation("Project with ID {ProjectId} deleted successfully", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting project with ID: {ProjectId}", id);
                return StatusCode(500, new
                {
                    Error = "Internal server error",
                    Message = "An error occurred while deleting the project"
                });
            }
        }

        /// <summary>
        /// Mark project as completed
        /// </summary>
        /// <param name="id">Project ID</param>
        /// <param name="request">Completion data</param>
        /// <returns>Updated project</returns>
        [HttpPatch("{id}/complete")]
        [ProducesResponseType(typeof(ProjectDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ProjectDto>> CompleteProject(int id, [FromBody] CompleteProjectRequest request)
        {
            try
            {
                _logger.LogInformation("Marking project {ProjectId} as completed", id);

                if (id <= 0)
                {
                    return BadRequest("Project ID must be greater than 0");
                }

                var existingProject = await _projectService.GetProjectByIdAsync(id);
                if (existingProject == null)
                {
                    _logger.LogWarning("Project with ID {ProjectId} not found for completion", id);
                    return NotFound(new { Message = $"Project with ID {id} not found" });
                }

                // Apply completion data using AutoMapper
                _mapper.Map(request, existingProject);

                var updatedProject = await _projectService.UpdateProjectAsync(existingProject);
                var projectDto = _mapper.Map<ProjectDto>(updatedProject);

                _logger.LogInformation("Project {ProjectId} marked as completed", id);
                return Ok(projectDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing project with ID: {ProjectId}", id);
                return StatusCode(500, new
                {
                    Error = "Internal server error",
                    Message = "An error occurred while completing the project"
                });
            }
        }

        /// <summary>
        /// Get active projects for a portfolio
        /// </summary>
        /// <param name="portfolioId">Portfolio ID</param>
        /// <returns>List of active projects</returns>
        [HttpGet("portfolio/{portfolioId}/active")]
        [ProducesResponseType(typeof(IEnumerable<ProjectDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetActiveProjects(int portfolioId)
        {
            try
            {
                if (!await _portfolioService.PortfolioExistsAsync(portfolioId))
                {
                    return NotFound(new { Message = $"Portfolio with ID {portfolioId} not found" });
                }

                var projects = await _projectService.GetProjectsByPortfolioIdAsync(portfolioId);
                var activeProjects = projects.Where(p => !p.IsCompleted);
                var projectDtos = _mapper.Map<IEnumerable<ProjectDto>>(activeProjects);

                return Ok(projectDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active projects for portfolio: {PortfolioId}", portfolioId);
                return StatusCode(500, new
                {
                    Error = "Internal server error",
                    Message = "An error occurred while retrieving active projects"
                });
            }
        }

        /// <summary>
        /// Get completed projects for a portfolio
        /// </summary>
        /// <param name="portfolioId">Portfolio ID</param>
        /// <returns>List of completed projects</returns>
        [HttpGet("portfolio/{portfolioId}/completed")]
        [ProducesResponseType(typeof(IEnumerable<ProjectDto>), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetCompletedProjects(int portfolioId)
        {
            try
            {
                if (!await _portfolioService.PortfolioExistsAsync(portfolioId))
                {
                    return NotFound(new { Message = $"Portfolio with ID {portfolioId} not found" });
                }

                var projects = await _projectService.GetProjectsByPortfolioIdAsync(portfolioId);
                var completedProjects = projects.Where(p => p.IsCompleted);
                var projectDtos = _mapper.Map<IEnumerable<ProjectDto>>(completedProjects);

                return Ok(projectDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving completed projects for portfolio: {PortfolioId}", portfolioId);
                return StatusCode(500, new
                {
                    Error = "Internal server error",
                    Message = "An error occurred while retrieving completed projects"
                });
            }
        }
    }
}
