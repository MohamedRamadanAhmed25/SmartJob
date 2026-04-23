using AutoMapper;
using SmartJob.API.DTOs.Reports;
using SmartJob.API.Models;

namespace SmartJob.API.Mappings;

public class ReportMappingProfile : Profile
{
    public ReportMappingProfile()
    {
        CreateMap<Report, ReportDto>()
            .ForMember(dest => dest.ReportType, opt => opt.MapFrom(src => src.ReportType.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
