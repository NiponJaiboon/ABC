#nullable enable
using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class SkillService : ISkillService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<Skill> _skillRepository;
    private readonly ILogger<SkillService> _logger;

    public SkillService(
        IUnitOfWork unitOfWork,
        IGenericRepository<Skill> skillRepository,
        ILogger<SkillService> logger)
    {
        _unitOfWork = unitOfWork;
        _skillRepository = skillRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<Skill>> GetAllSkillsAsync()
    {
        _logger.LogInformation("Retrieving all skills");
        return await _skillRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Skill>> GetSkillsByCategoryAsync(string category)
    {
        _logger.LogInformation("Retrieving skills for category: {Category}", category);
        var allSkills = await _skillRepository.GetAllAsync();
        return allSkills.Where(s => s.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Skill?> GetSkillByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving skill with ID: {SkillId}", id);
        return await _skillRepository.GetByIdAsync(id);
    }

    public async Task<Skill> CreateSkillAsync(Skill skill)
    {
        _logger.LogInformation("Creating new skill: {SkillName}", skill.Name);
        
        if (string.IsNullOrWhiteSpace(skill.Name))
            throw new ArgumentException("Skill name is required");

        if (string.IsNullOrWhiteSpace(skill.Category))
            throw new ArgumentException("Skill category is required");

        skill.CreatedAt = DateTime.UtcNow;

        await _skillRepository.AddAsync(skill);
        await _unitOfWork.CommitAsync();

        _logger.LogInformation("Skill created with ID: {SkillId}", skill.Id);
        return skill;
    }

    public async Task<Skill> UpdateSkillAsync(Skill skill)
    {
        _logger.LogInformation("Updating skill with ID: {SkillId}", skill.Id);

        if (!await SkillExistsAsync(skill.Id))
            throw new ArgumentException($"Skill with ID {skill.Id} not found");

        await _skillRepository.UpdateAsync(skill);
        await _unitOfWork.CommitAsync();

        _logger.LogInformation("Skill updated successfully: {SkillId}", skill.Id);
        return skill;
    }

    public async Task<bool> DeleteSkillAsync(int id)
    {
        _logger.LogInformation("Deleting skill with ID: {SkillId}", id);

        if (!await SkillExistsAsync(id))
            return false;

        await _skillRepository.DeleteAsync(id);
        await _unitOfWork.CommitAsync();

        _logger.LogInformation("Skill deleted successfully: {SkillId}", id);
        return true;
    }

    public async Task<bool> SkillExistsAsync(int id)
    {
        return await _skillRepository.GetByIdAsync(id) != null;
    }

    public async Task<IEnumerable<string>> GetSkillCategoriesAsync()
    {
        _logger.LogInformation("Retrieving skill categories");
        var skills = await _skillRepository.GetAllAsync();
        return skills.Select(s => s.Category).Distinct().OrderBy(c => c);
    }
}
