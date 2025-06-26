using System.ComponentModel.DataAnnotations;

namespace Application.Dtos
{
    public class UpdatePortfolioRequest
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; }

        public bool IsPublic { get; set; }

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;
    }
}
