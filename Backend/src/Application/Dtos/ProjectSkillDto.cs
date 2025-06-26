using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Dtos
{
    public class ProjectSkillDto
    {
        public int ProjectId { get; set; }
        public int SkillId { get; set; }
        public int? ProficiencyLevel { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }

        // Additional info
        public string ProjectTitle { get; set; }
        public string SkillName { get; set; }
        public string SkillCategory { get; set; }
    }
}
