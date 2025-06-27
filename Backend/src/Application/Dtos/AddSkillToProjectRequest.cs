using System.ComponentModel.DataAnnotations;

namespace Application.Dtos;

public class AddSkillToProjectRequest
{
    [Required]
    public int SkillId { get; set; }

    [Range(1, 5, ErrorMessage = "Proficiency level must be between 1 and 5")]
    public int ProficiencyLevel { get; set; } = 3;

    public bool IsPrimary { get; set; } = false;
}
