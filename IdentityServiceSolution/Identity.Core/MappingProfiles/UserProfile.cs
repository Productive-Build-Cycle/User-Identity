using AutoMapper;
using Identity.Core.Dtos.Auth;
using Identity.Core.Dtos.Users;
using Identity.Core.Domain.Entities;

namespace Identity.Core.MappingProfiles;

public class UserProfile : Profile
{
    public UserProfile() 
    {
        CreateMap<RegisterRequest, ApplicationUser>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.Email))
            .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName))
            .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
            .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => s.PhoneNumber))
            .ForMember(d => d.Id, o => o.Ignore());

        CreateMap<UpdateUserRequest, ApplicationUser>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.Email))
            .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName))
            .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
            .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => s.PhoneNumber));

        CreateMap<ApplicationUser, UserResponse>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
            .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName))
            .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName))
            .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => s.PhoneNumber))
            .ForMember(d => d.Roles, o => o.Ignore());
    }
}
