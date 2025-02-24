using app.entities;
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
                .ForMember(dto => dto.FullName, p => p.MapFrom(src => string.Join(",", new[] { src.FirstName, src.MiddleName, src.LastName }
                .Where(x => !string.IsNullOrEmpty(x))
                )
            ))
                .ForMember(dto => dto.BirthDate, p => p.MapFrom(src => src.BirthDate.ToString("yyyy-MM-dd")))
                .ReverseMap();

            CreateMap<Account, AccountDto>()
                .ForMember(dto => dto.AccountRoleDto, p => p.MapFrom(src => src.AccountRole))
                .ReverseMap();

        }
    }
}
