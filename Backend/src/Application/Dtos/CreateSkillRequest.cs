using System.ComponentModel.DataAnnotations;

namespace Application.Dtos;

public class CreateSkillRequest
{
    [Required(ErrorMessage = "Skill name is required")]
    [StringLength(100, ErrorMessage = "Skill name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
    public string Category { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string Description { get; set; } = string.Empty;
}
