using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.CodeAnalysis;

#nullable enable

namespace Core.Entities
{
    [Table("Portfolios")]
    public class Portfolio
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsPublic { get; set; } = false;

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ✅ เปลี่ยนเป็น nullable
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
