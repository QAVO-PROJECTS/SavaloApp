using AutoMapper;
using SavaloApp.Application.Dtos.GoalSection;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Application.Profiles;

public class GoalSectionProfile : Profile
{
    public GoalSectionProfile()
    {
        CreateMap<GoalSection, GoalSectionDto>();

        CreateMap<CreateGoalSectionDto, GoalSection>()
            .ForMember(x => x.Icon, opt => opt.Ignore());

        CreateMap<UpdateGoalSectionDto, GoalSection>()
            .ForMember(x => x.Icon, opt => opt.Ignore());
    }
}