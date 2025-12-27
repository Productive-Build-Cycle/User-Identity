using AutoMapper;
using Identity.Core.Application.DTOs.Auth;
using Identity.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.Application.Mapper
{
    public class UserProfile : Profile
    {
        public UserProfile() 
        {
            // RegisterRequest -> ApplicationUser
            CreateMap<RegisterRequest, ApplicationUser>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.UserName))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
            .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => s.PhoneNumber))
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.PasswordHash, o => o.Ignore());

            // UpdateUserRequest -> ApplicationUser (for updates, only map non-null values)
            CreateMap<UpdateUserRequest, ApplicationUser>()
                .ForAllMembers(opt =>
                    opt.Condition((src, dest, srcMember) => srcMember != null));

            // ApplicationUser -> UserInfoResponse
            CreateMap<ApplicationUser, AuthRespose.UserInfoResponse>()
                .ForMember(d => d.Roles, o => o.Ignore()); // roles separate

        }
    }
}
