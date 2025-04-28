using app.entities;
using app.services.Authorizations;
using app.shared.Dto.Account;
using app.shared.Dto.PersonalDetail;
using AutoMapper;

namespace app.services
{
    public sealed class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<PersonalDetail, ViewPersonalDetailDto>()
                .ForMember(dto => dto.FullName, 
                    p => p.MapFrom(src => string.Join(",", new[] { src.FirstName, src.MiddleName, src.LastName }
                        .Where(x => !string.IsNullOrEmpty(x))
                )
            ))
                .ForMember(dto => dto.BirthDate, p => p.MapFrom(src => src.BirthDate.ToString("yyyy-MM-dd")))
                .ReverseMap();

            CreateMap<PersonalDetail, CreatePersonalDetailDto>()
                .ForMember(dto => dto.UserName, p => p.MapFrom(src => src.Account.UserName))
                .ForMember(dto => dto.Password, p => p.MapFrom(src => src.Account.Password))
                .ForMember(dto => dto.BirthDate, p => p.MapFrom(src => src.BirthDate.ToString("yyyy-MM-dd")))
                .ReverseMap()
                .ForMember(src => src.BirthDate, opt => opt.MapFrom(dto => DateTime.Parse(dto.BirthDate)));

            CreateMap<Account, AccountDto>()
                .ForMember(dto => dto.AccountRoleDto, p => p.MapFrom(src => src.AccountRole))
                .ReverseMap();

            CreateMap<AccountSecurity, AccountSecurityDto>().ReverseMap();
            CreateMap<AccountRole, AccountRoleDto>().ReverseMap();
            CreateMap<Permission, AccountRoleDto>().ReverseMap();
        }
    }
}
