using AutoMapper;
using SmartJob.API.DTOs.Jobs;
using SmartJob.API.Models;

namespace SmartJob.API.Mappings;

public class JobMappingProfile : Profile
{
    public JobMappingProfile()
    {
        CreateMap<Job, JobDto>()
            .ForMember(dest => dest.EmployerName, opt => opt.MapFrom(src => src.Employer != null ? src.Employer.Name : string.Empty))
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Employer != null && src.Employer.EmployerProfile != null ? src.Employer.EmployerProfile.CompanyName : string.Empty))
            .ForMember(dest => dest.CompanyLogoUrl, opt => opt.MapFrom(src => src.Employer != null && src.Employer.EmployerProfile != null ? src.Employer.EmployerProfile.CompanyLogoUrl : string.Empty))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ApplicationCount, opt => opt.MapFrom(src => src.Applications != null ? src.Applications.Count : 0));
    }
}
