using AutoMapper;
using SmartJob.API.DTOs.Auth;
using SmartJob.API.Models;

namespace SmartJob.API.Mappings;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<User, AuthUserDto>();
    }
}
