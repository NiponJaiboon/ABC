namespace Application.Dtos;

public class ProjectSkillDto
{
    public int ProjectId { get; set; }
    public int SkillId { get; set; }
    public int ProficiencyLevel { get; set; } // 1-5 scale
    public bool IsPrimary { get; set; }
    public SkillDto Skill { get; set; } = new();
}
