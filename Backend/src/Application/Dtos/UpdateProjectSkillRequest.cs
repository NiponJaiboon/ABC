using System.ComponentModel.DataAnnotations;

namespace Application.Dtos;

public class UpdateProjectSkillRequest
{
    [Range(1, 5, ErrorMessage = "Proficiency level must be between 1 and 5")]
    public int ProficiencyLevel { get; set; }

    public bool IsPrimary { get; set; }
}
