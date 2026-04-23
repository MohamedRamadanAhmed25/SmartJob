using AutoMapper;
using SmartJob.API.DTOs.Users;
using SmartJob.API.Models;

namespace SmartJob.API.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserProfileDto>();
        CreateMap<SeekerProfile, SeekerProfileDto>();
        CreateMap<EmployerProfile, EmployerProfileDto>();
    }
}
