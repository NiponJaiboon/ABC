using Core.Entities;

#nullable enable

namespace Core.Interfaces;

public interface ISkillService
{
    Task<IEnumerable<Skill>> GetAllSkillsAsync();
    Task<IEnumerable<Skill>> GetSkillsByCategoryAsync(string category);
    Task<Skill?> GetSkillByIdAsync(int id);
    Task<Skill> CreateSkillAsync(Skill skill);
    Task<Skill> UpdateSkillAsync(Skill skill);
    Task<bool> DeleteSkillAsync(int id);
    Task<bool> SkillExistsAsync(int id);
    Task<IEnumerable<string>> GetSkillCategoriesAsync();
}
