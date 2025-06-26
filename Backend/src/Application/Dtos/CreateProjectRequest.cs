using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos
{
    public class CreateProjectRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Description { get; set; }

        [MaxLength(500)]
        public string ProjectUrl { get; set; }

        [MaxLength(500)]
        public string GitHubUrl { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCompleted { get; set; } = false;

        [Required]
        public int PortfolioId { get; set; }
    }
}
