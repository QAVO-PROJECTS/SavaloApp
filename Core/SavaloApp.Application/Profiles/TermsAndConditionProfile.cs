using AutoMapper;
using SavaloApp.Application.Dtos.TermsAndCondition;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Application.Profiles;

public class TermsAndConditionProfile:Profile
{
    public TermsAndConditionProfile()
    {
        CreateMap<TermsAndCondition, TermsAndConditionDto>();
        CreateMap<CreateTermsAndConditionDto, TermsAndCondition>();
        CreateMap<UpdateTermsAndConditionDto, TermsAndCondition>();
    }
}