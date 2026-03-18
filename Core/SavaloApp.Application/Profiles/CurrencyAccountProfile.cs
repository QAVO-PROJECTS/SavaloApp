using AutoMapper;
using SavaloApp.Application.Dtos.CurrencyAccount;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Application.Profiles;

public class CurrencyAccountProfile : Profile
{
    public CurrencyAccountProfile()
    {
        CreateMap<CurrencyAccount, CurrencyAccountDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

        CreateMap<CreateCurrencyAccountDto, CurrencyAccount>()
            .ForMember(dest => dest.Icon, opt => opt.Ignore());

        CreateMap<UpdateCurrencyAccountDto, CurrencyAccount>()
            .ForMember(dest => dest.Icon, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}