using api.entities;
using api.repository.Authorization;
using api.shared.Dto;
using AutoMapper;

namespace api.service
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>()
                       .ForMember(dest => dest.UserRoles, opt => opt.MapFrom(src => src.UserRoles))
                       .ReverseMap();

            CreateMap<UserRoles, UserRolesDto>().ReverseMap();

            CreateMap<Permission, PermissionDto>();
        }
    }
}
