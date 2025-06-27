// Backend/src/Core/Entities/Project.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable

namespace Core.Entities
{
    [Table("Projects")]
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ProjectUrl { get; set; }

        [MaxLength(500)]
        public string? GitHubUrl { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ✅ เปลี่ยนเป็น nullable
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("Portfolio")]
        public int PortfolioId { get; set; }

        public Portfolio Portfolio { get; set; } = null!;
        public ICollection<ProjectSkill> ProjectSkills { get; set; } = new List<ProjectSkill>();
    }
}
