using AutoMapper;
using SmartJob.API.DTOs.Interviews;
using SmartJob.API.Models;

namespace SmartJob.API.Mappings;

public class InterviewMappingProfile : Profile
{
    public InterviewMappingProfile()
    {
        CreateMap<Interview, InterviewDto>()
            .ForMember(dest => dest.JobTitle, opt => opt.MapFrom(src => src.Application != null && src.Application.Job != null ? src.Application.Job.Title : string.Empty))
            .ForMember(dest => dest.SeekerName, opt => opt.MapFrom(src => src.Application != null && src.Application.Seeker != null ? src.Application.Seeker.Name : string.Empty))
            .ForMember(dest => dest.EmployerName, opt => opt.MapFrom(src => src.Application != null && src.Application.Job != null && src.Application.Job.Employer != null ? src.Application.Job.Employer.Name : string.Empty))
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Application != null && src.Application.Job != null && src.Application.Job.Employer != null && src.Application.Job.Employer.EmployerProfile != null ? src.Application.Job.Employer.EmployerProfile.CompanyName : string.Empty))
            .ForMember(dest => dest.Mode, opt => opt.MapFrom(src => src.Mode.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
