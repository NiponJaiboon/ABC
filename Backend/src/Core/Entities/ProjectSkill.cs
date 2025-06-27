using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities;

[Table("ProjectSkills")]
public class ProjectSkill
{
    public int Id { get; set; }
    
    [ForeignKey("Project")]
    public int ProjectId { get; set; }

    [ForeignKey("Skill")]
    public int SkillId { get; set; }

    public int ProficiencyLevel { get; set; }
    public bool IsPrimary { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Project Project { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
