using AutoMapper;
using SavaloApp.Application.Dtos.Icon;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Application.Profiles;

public class IconProfile:Profile
{
    public IconProfile()
    {
        CreateMap<Icon, IconDto>();

        CreateMap<CreateIconDto, Icon>()
            .ForMember(x => x.IconName, opt => opt.Ignore());

    }
}