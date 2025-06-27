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
            // Portfolio Mappings
            // Portfolio Mappings
            CreateMap<Portfolio, PortfolioDto>().ReverseMap();
            CreateMap<Portfolio, PortfolioWithProjectsDto>()
                .IncludeBase<Portfolio, PortfolioDto>();

            CreateMap<CreatePortfolioRequest, Portfolio>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Projects, opt => opt.Ignore());

            CreateMap<UpdatePortfolioRequest, Portfolio>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Projects, opt => opt.Ignore());


            // Project Mappings
            CreateMap<Project, ProjectDto>()
                .ForMember(dest => dest.PortfolioTitle, opt => opt.MapFrom(src => src.Portfolio.Title))
                .ReverseMap();

            CreateMap<CreateProjectRequest, Project>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Portfolio, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectSkills, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src =>
                    src.StartDate != default(DateTime) ? src.StartDate : DateTime.UtcNow));


            CreateMap<UpdateProjectRequest, Project>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Portfolio, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectSkills, opt => opt.Ignore());

            CreateMap<CompleteProjectRequest, Project>()
                .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src =>
                    src.EndDate ?? DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opt => opt.Condition((src, dest, member) =>
                    member != null && (
                        opt.DestinationMember.Name == "IsCompleted" ||
                        opt.DestinationMember.Name == "EndDate" ||
                        opt.DestinationMember.Name == "UpdatedAt"
                    )));

            // Skill Mappings
            CreateMap<Skill, SkillDto>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<CreateSkillRequest, Skill>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ProjectSkills, opt => opt.Ignore());

            // ProjectSkill Mappings
            CreateMap<ProjectSkill, ProjectSkillDto>()
                .ForMember(dest => dest.Skill, opt => opt.MapFrom(src => src.Skill));
            CreateMap<AddSkillToProjectRequest, ProjectSkill>()
                .ForMember(dest => dest.ProjectId, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore())
                .ForMember(dest => dest.Skill, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());
        }
    }
}
