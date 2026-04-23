using AutoMapper;
using SmartJob.API.DTOs.Chats;
using SmartJob.API.Models;

namespace SmartJob.API.Mappings;

public class ChatMappingProfile : Profile
{
    public ChatMappingProfile()
    {
        CreateMap<Chat, ChatDto>()
            .ForMember(dest => dest.SeekerName, opt => opt.MapFrom(src => src.Seeker != null ? src.Seeker.Name : string.Empty))
            .ForMember(dest => dest.SeekerAvatarUrl, opt => opt.MapFrom(src => src.Seeker != null ? src.Seeker.AvatarUrl : string.Empty))
            .ForMember(dest => dest.EmployerName, opt => opt.MapFrom(src => src.Employer != null ? src.Employer.Name : string.Empty))
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Employer != null && src.Employer.EmployerProfile != null ? src.Employer.EmployerProfile.CompanyName : string.Empty))
            .ForMember(dest => dest.LastMessage, opt => opt.MapFrom(src => src.Messages.Any() ? src.Messages.First().Content : string.Empty))
            .ForMember(dest => dest.UnreadCount, opt => opt.Ignore()); 

        CreateMap<Message, MessageDto>()
            .ForMember(dest => dest.SenderName, opt => opt.MapFrom(src => src.Sender != null ? src.Sender.Name : string.Empty))
            .ForMember(dest => dest.SenderAvatarUrl, opt => opt.MapFrom(src => src.Sender != null ? src.Sender.AvatarUrl : string.Empty));
    }
}
