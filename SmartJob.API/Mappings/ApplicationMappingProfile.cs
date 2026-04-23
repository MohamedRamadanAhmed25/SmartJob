using AutoMapper;
using SmartJob.API.DTOs.Applications;
using SmartJob.API.Models;

namespace SmartJob.API.Mappings;

public class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        CreateMap<Application, ApplicationDto>()
            .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.Job != null ? src.Job.Title : string.Empty))
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Job != null && src.Job.Employer != null && src.Job.Employer.EmployerProfile != null ? src.Job.Employer.EmployerProfile.CompanyName : string.Empty))
            .ForMember(dest => dest.SeekerName, opt => opt.MapFrom(src => src.Seeker != null ? src.Seeker.Name : string.Empty))
            .ForMember(dest => dest.SeekerAvatarUrl, opt => opt.MapFrom(src => src.Seeker != null ? src.Seeker.AvatarUrl : string.Empty))
            .ForMember(dest => dest.ResumeFileName, opt => opt.MapFrom(src => src.Resume != null ? src.Resume.FileName : string.Empty))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
