using Core.Entities;

#nullable enable

namespace Core.Interfaces;

public interface IProjectSkillService
{
    Task<IEnumerable<ProjectSkill>> GetProjectSkillsAsync(int projectId);
    Task<ProjectSkill?> GetProjectSkillAsync(int projectId, int skillId);
    Task<ProjectSkill> AddSkillToProjectAsync(int projectId, int skillId, int proficiencyLevel, bool isPrimary = false);
    Task<ProjectSkill> UpdateProjectSkillAsync(int projectId, int skillId, int proficiencyLevel, bool isPrimary);
    Task<bool> RemoveSkillFromProjectAsync(int projectId, int skillId);
    Task<IEnumerable<Project>> GetProjectsBySkillAsync(int skillId);
    Task<bool> ProjectHasSkillAsync(int projectId, int skillId);
}
