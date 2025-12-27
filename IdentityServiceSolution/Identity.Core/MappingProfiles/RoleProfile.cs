using AutoMapper;
using Identity.Core.Dtos.Roles;
using Identity.Core.Domain.Entities;

namespace Identity.Core.MappingProfiles;

public class RoleProfile : Profile
{
    public RoleProfile()
    {
        CreateMap<ApplicationRole, RoleResponse>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
            .ForMember(d => d.Description, o => o.MapFrom(s => s.Description));

        CreateMap<AddRoleRequest, ApplicationRole>()
            .ForMember(d => d.Name, o => o.MapFrom(s => s.Name))
            .ForMember(d => d.Description, o => o.MapFrom(s => s.Description))
            .ForMember(d => d.Id, o => o.Ignore());

        CreateMap<UpdateRoleRequest, ApplicationRole>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Description, o => o.MapFrom(s => s.Description))
            .ForMember(d => d.Name, o => o.MapFrom(s => s.Name));
    }
}
