using AutoMapper;
using SavaloApp.Application.Dtos.CategoryTransaction;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Application.Profiles;

public class CategoryTransactionProfile : Profile
{
    public CategoryTransactionProfile()
    {
        CreateMap<CategoryTransaction, CategoryTransactionDto>();

        CreateMap<CreateCategoryTransactionDto, CategoryTransaction>();

        CreateMap<UpdateCategoryTransactionDto, CategoryTransaction>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}