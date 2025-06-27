using Application.Dtos;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillController : ControllerBase
{
    private readonly ISkillService _skillService;
    private readonly IMapper _mapper;
    private readonly ILogger<SkillController> _logger;

    public SkillController(
        ISkillService skillService,
        IMapper mapper,
        ILogger<SkillController> logger)
    {
        _skillService = skillService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Get all skills
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SkillDto>), 200)]
    public async Task<ActionResult<IEnumerable<SkillDto>>> GetAllSkills()
    {
        try
        {
            var skills = await _skillService.GetAllSkillsAsync();
            var skillDtos = _mapper.Map<IEnumerable<SkillDto>>(skills);
            return Ok(skillDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all skills");
            return StatusCode(500, new { Message = "An error occurred while retrieving skills" });
        }
    }

    /// <summary>
    /// Get skills by category
    /// </summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(IEnumerable<SkillDto>), 200)]
    public async Task<ActionResult<IEnumerable<SkillDto>>> GetSkillsByCategory(string category)
    {
        try
        {
            var skills = await _skillService.GetSkillsByCategoryAsync(category);
            var skillDtos = _mapper.Map<IEnumerable<SkillDto>>(skills);
            return Ok(skillDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving skills for category: {Category}", category);
            return StatusCode(500, new { Message = "An error occurred while retrieving skills" });
        }
    }

    /// <summary>
    /// Get skill categories
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IEnumerable<string>), 200)]
    public async Task<ActionResult<IEnumerable<string>>> GetSkillCategories()
    {
        try
        {
            var categories = await _skillService.GetSkillCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving skill categories");
            return StatusCode(500, new { Message = "An error occurred while retrieving skill categories" });
        }
    }

    /// <summary>
    /// Get skill by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SkillDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<SkillDto>> GetSkillById(int id)
    {
        try
        {
            var skill = await _skillService.GetSkillByIdAsync(id);
            if (skill == null)
                return NotFound(new { Message = $"Skill with ID {id} not found" });

            var skillDto = _mapper.Map<SkillDto>(skill);
            return Ok(skillDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving skill with ID: {SkillId}", id);
            return StatusCode(500, new { Message = "An error occurred while retrieving the skill" });
        }
    }

    /// <summary>
    /// Create new skill
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SkillDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<SkillDto>> CreateSkill([FromBody] CreateSkillRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var skill = _mapper.Map<Skill>(request);
            var createdSkill = await _skillService.CreateSkillAsync(skill);
            var skillDto = _mapper.Map<SkillDto>(createdSkill);

            return CreatedAtAction(
                nameof(GetSkillById),
                new { id = createdSkill.Id },
                skillDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating skill");
            return StatusCode(500, new { Message = "An error occurred while creating the skill" });
        }
    }

    /// <summary>
    /// Delete skill
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteSkill(int id)
    {
        try
        {
            var deleted = await _skillService.DeleteSkillAsync(id);
            if (!deleted)
                return NotFound(new { Message = $"Skill with ID {id} not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting skill with ID: {SkillId}", id);
            return StatusCode(500, new { Message = "An error occurred while deleting the skill" });
        }
    }
}
