using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; }
        public string ProjectUrl { get; set; }
        public string GitHubUrl { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Foreign Key
        public int PortfolioId { get; set; }

        // Additional info from navigation properties
        public string PortfolioTitle { get; set; }
        public ICollection<ProjectSkillDto> ProjectSkills { get; set; } = new List<ProjectSkillDto>();
    }
}
