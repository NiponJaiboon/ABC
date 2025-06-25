using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Dtos;
using AutoMapper;
using Core.Entities;

namespace Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Portfolio
            CreateMap<Portfolio, PortfolioDto>().ReverseMap();
            CreateMap<CreatePortfolioDto, Portfolio>();
            CreateMap<UpdatePortfolioDto, Portfolio>();

            // Project
            CreateMap<Project, ProjectDto>().ReverseMap();
            CreateMap<CreateProjectDto, Project>();
            CreateMap<UpdateProjectDto, Project>();

            // Skill
            CreateMap<Skill, SkillDto>().ReverseMap();
            CreateMap<CreateSkillDto, Skill>();
            CreateMap<UpdateSkillDto, Skill>();

            // ProjectSkill
            CreateMap<ProjectSkill, ProjectSkillDto>().ReverseMap();
        }
    }
}