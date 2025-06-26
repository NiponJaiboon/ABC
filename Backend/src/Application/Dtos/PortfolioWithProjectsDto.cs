using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class PortfolioWithProjectsDto : PortfolioDto
    {
        public ICollection<ProjectDto> Projects { get; set; } = new List<ProjectDto>();
        public int ProjectCount => Projects.Count;
        public int CompletedProjectCount => Projects.Count(p => p.IsCompleted);
        public int ActiveProjectCount => Projects.Count(p => !p.IsCompleted);
    }
}
