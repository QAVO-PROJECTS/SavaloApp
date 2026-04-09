using AutoMapper;
using SavaloApp.Application.Dtos.GoalTransaction;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Application.Profiles;

public class GoalTransactionProfile : Profile
{
    public GoalTransactionProfile()
    {
        CreateMap<GoalTransaction, GoalTransactionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

        CreateMap<CreateGoalTransactionDto, GoalTransaction>()
            .ForMember(dest => dest.GoalId, opt => opt.Ignore());
    }
}