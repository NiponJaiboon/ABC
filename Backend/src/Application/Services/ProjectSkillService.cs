#nullable enable
using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ProjectSkillService : IProjectSkillService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<ProjectSkill> _projectSkillRepository;
    private readonly IProjectService _projectService;
    private readonly ISkillService _skillService;
    private readonly ILogger<ProjectSkillService> _logger;

    public ProjectSkillService(
        IUnitOfWork unitOfWork,
        IGenericRepository<ProjectSkill> projectSkillRepository,
        IProjectService projectService,
        ISkillService skillService,
        ILogger<ProjectSkillService> logger)
    {
        _unitOfWork = unitOfWork;
        _projectSkillRepository = projectSkillRepository;
        _projectService = projectService;
        _skillService = skillService;
        _logger = logger;
    }

    public async Task<IEnumerable<ProjectSkill>> GetProjectSkillsAsync(int projectId)
    {
        _logger.LogInformation("Retrieving skills for project: {ProjectId}", projectId);
        var allProjectSkills = await _projectSkillRepository.GetAllAsync();
        return allProjectSkills.Where(ps => ps.ProjectId == projectId);
    }

    public async Task<ProjectSkill?> GetProjectSkillAsync(int projectId, int skillId)
    {
        _logger.LogInformation("Retrieving project skill: Project {ProjectId}, Skill {SkillId}", projectId, skillId);
        var allProjectSkills = await _projectSkillRepository.GetAllAsync();
        return allProjectSkills.FirstOrDefault(ps => ps.ProjectId == projectId && ps.SkillId == skillId);
    }

    public async Task<ProjectSkill> AddSkillToProjectAsync(int projectId, int skillId, int proficiencyLevel, bool isPrimary = false)
    {
        _logger.LogInformation("Adding skill {SkillId} to project {ProjectId}", skillId, projectId);

        // Validate project exists
        if (!await _projectService.ProjectExistsAsync(projectId))
            throw new ArgumentException($"Project with ID {projectId} not found");

        // Validate skill exists
        if (!await _skillService.SkillExistsAsync(skillId))
            throw new ArgumentException($"Skill with ID {skillId} not found");

        // Check if skill already exists for project
        if (await ProjectHasSkillAsync(projectId, skillId))
            throw new ArgumentException($"Skill {skillId} is already assigned to project {projectId}");

        // Validate proficiency level
        if (proficiencyLevel < 1 || proficiencyLevel > 5)
            throw new ArgumentException("Proficiency level must be between 1 and 5");

        var projectSkill = new ProjectSkill
        {
            ProjectId = projectId,
            SkillId = skillId,
            ProficiencyLevel = proficiencyLevel,
            IsPrimary = isPrimary
        };

        await _projectSkillRepository.AddAsync(projectSkill);
        await _unitOfWork.CommitAsync();

        _logger.LogInformation("Skill {SkillId} added to project {ProjectId} successfully", skillId, projectId);
        return projectSkill;
    }

    public async Task<ProjectSkill> UpdateProjectSkillAsync(int projectId, int skillId, int proficiencyLevel, bool isPrimary)
    {
        _logger.LogInformation("Updating project skill: Project {ProjectId}, Skill {SkillId}", projectId, skillId);

        var existingProjectSkill = await GetProjectSkillAsync(projectId, skillId);
        if (existingProjectSkill == null)
            throw new ArgumentException($"Skill {skillId} is not assigned to project {projectId}");

        // Validate proficiency level
        if (proficiencyLevel < 1 || proficiencyLevel > 5)
            throw new ArgumentException("Proficiency level must be between 1 and 5");

        existingProjectSkill.ProficiencyLevel = proficiencyLevel;
        existingProjectSkill.IsPrimary = isPrimary;

        await _projectSkillRepository.UpdateAsync(existingProjectSkill);
        await _unitOfWork.CommitAsync();

        _logger.LogInformation("Project skill updated successfully: Project {ProjectId}, Skill {SkillId}", projectId, skillId);
        return existingProjectSkill;
    }

    public async Task<bool> RemoveSkillFromProjectAsync(int projectId, int skillId)
    {
        _logger.LogInformation("Removing skill {SkillId} from project {ProjectId}", skillId, projectId);

        var projectSkill = await GetProjectSkillAsync(projectId, skillId);
        if (projectSkill == null)
            return false;

        // For now, throw NotImplementedException as this requires direct DbContext access
        // In a real implementation, we would need to add an Id field to ProjectSkill entity
        // or create a custom repository method for composite key deletion
        throw new NotImplementedException("Delete by composite key not yet implemented. Consider adding Id to ProjectSkill entity.");
    }

    public async Task<IEnumerable<Project>> GetProjectsBySkillAsync(int skillId)
    {
        _logger.LogInformation("Retrieving projects that use skill: {SkillId}", skillId);
        
        var projectSkills = await _projectSkillRepository.GetAllAsync();
        var projectIds = projectSkills.Where(ps => ps.SkillId == skillId).Select(ps => ps.ProjectId);

        var projects = new List<Project>();
        foreach (var projectId in projectIds)
        {
            var project = await _projectService.GetProjectByIdAsync(projectId);
            if (project != null)
                projects.Add(project);
        }

        return projects;
    }

    public async Task<bool> ProjectHasSkillAsync(int projectId, int skillId)
    {
        var projectSkill = await GetProjectSkillAsync(projectId, skillId);
        return projectSkill != null;
    }
}
