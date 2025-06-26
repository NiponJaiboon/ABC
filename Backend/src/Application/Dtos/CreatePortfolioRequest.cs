using System.ComponentModel.DataAnnotations;

namespace Application.Dtos
{
    public class CreatePortfolioRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; }

        public bool IsPublic { get; set; } = false;

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;
    }
}
