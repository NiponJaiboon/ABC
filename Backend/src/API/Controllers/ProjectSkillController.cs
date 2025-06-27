using Application.Dtos;
using AutoMapper;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/projects/{projectId}/skills")]
public class ProjectSkillController : ControllerBase
{
    private readonly IProjectSkillService _projectSkillService;
    private readonly IMapper _mapper;
    private readonly ILogger<ProjectSkillController> _logger;

    public ProjectSkillController(
        IProjectSkillService projectSkillService,
        IMapper mapper,
        ILogger<ProjectSkillController> logger)
    {
        _projectSkillService = projectSkillService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get all skills for a project
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProjectSkillDto>), 200)]
    public async Task<ActionResult<IEnumerable<ProjectSkillDto>>> GetProjectSkills(int projectId)
    {
        try
        {
            var projectSkills = await _projectSkillService.GetProjectSkillsAsync(projectId);
            var projectSkillDtos = _mapper.Map<IEnumerable<ProjectSkillDto>>(projectSkills);
            return Ok(projectSkillDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving skills for project: {ProjectId}", projectId);
            return StatusCode(500, new { Message = "An error occurred while retrieving project skills" });
        }
    }

    /// <summary>
    /// Add skill to project
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProjectSkillDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ProjectSkillDto>> AddSkillToProject(
        int projectId, 
        [FromBody] AddSkillToProjectRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var projectSkill = await _projectSkillService.AddSkillToProjectAsync(
                projectId, 
                request.SkillId, 
                request.ProficiencyLevel, 
                request.IsPrimary);

            var projectSkillDto = _mapper.Map<ProjectSkillDto>(projectSkill);
            return CreatedAtAction(
                nameof(GetProjectSkill),
                new { projectId = projectId, skillId = request.SkillId },
                projectSkillDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding skill to project: {ProjectId}", projectId);
            return StatusCode(500, new { Message = "An error occurred while adding skill to project" });
        }
    }

    /// <summary>
    /// Get specific skill for a project
    /// </summary>
    [HttpGet("{skillId}")]
    [ProducesResponseType(typeof(ProjectSkillDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ProjectSkillDto>> GetProjectSkill(int projectId, int skillId)
    {
        try
        {
            var projectSkill = await _projectSkillService.GetProjectSkillAsync(projectId, skillId);
            if (projectSkill == null)
                return NotFound(new { Message = $"Skill {skillId} not found for project {projectId}" });

            var projectSkillDto = _mapper.Map<ProjectSkillDto>(projectSkill);
            return Ok(projectSkillDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving skill {SkillId} for project {ProjectId}", skillId, projectId);
            return StatusCode(500, new { Message = "An error occurred while retrieving project skill" });
        }
    }

    /// <summary>
    /// Update project skill
    /// </summary>
    [HttpPut("{skillId}")]
    [ProducesResponseType(typeof(ProjectSkillDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ProjectSkillDto>> UpdateProjectSkill(
        int projectId, 
        int skillId, 
        [FromBody] UpdateProjectSkillRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedProjectSkill = await _projectSkillService.UpdateProjectSkillAsync(
                projectId, 
                skillId, 
                request.ProficiencyLevel, 
                request.IsPrimary);

            var projectSkillDto = _mapper.Map<ProjectSkillDto>(updatedProjectSkill);
            return Ok(projectSkillDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating skill {SkillId} for project {ProjectId}", skillId, projectId);
            return StatusCode(500, new { Message = "An error occurred while updating project skill" });
        }
    }

    /// <summary>
    /// Remove skill from project
    /// </summary>
    [HttpDelete("{skillId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> RemoveSkillFromProject(int projectId, int skillId)
    {
        try
        {
            var removed = await _projectSkillService.RemoveSkillFromProjectAsync(projectId, skillId);
            if (!removed)
                return NotFound(new { Message = $"Skill {skillId} not found for project {projectId}" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing skill {SkillId} from project {ProjectId}", skillId, projectId);
            return StatusCode(500, new { Message = "An error occurred while removing skill from project" });
        }
    }
}
