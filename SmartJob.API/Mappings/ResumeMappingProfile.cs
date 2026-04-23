using AutoMapper;
using SmartJob.API.DTOs.Resumes;
using SmartJob.API.Models;

namespace SmartJob.API.Mappings;

public class ResumeMappingProfile : Profile
{
    public ResumeMappingProfile()
    {
        CreateMap<Resume, ResumeDto>();
    }
}
