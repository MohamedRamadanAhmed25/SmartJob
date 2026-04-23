using AutoMapper;
using SmartJob.API.DTOs.Notifications;
using SmartJob.API.Models;

namespace SmartJob.API.Mappings;

public class NotificationMappingProfile : Profile
{
    public NotificationMappingProfile()
    {
        CreateMap<Notification, NotificationDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
    }
}
