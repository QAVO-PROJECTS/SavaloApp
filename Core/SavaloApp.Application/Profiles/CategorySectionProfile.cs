using AutoMapper;
using SavaloApp.Application.Dtos.CategorySection;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Application.Profiles;

public class CategorySectionProfile : Profile
{
    public CategorySectionProfile()
    {
        CreateMap<CategorySection, CategorySectionDto>();

        CreateMap<CreateCategorySectionDto, CategorySection>()
            .ForMember(x => x.Icon, opt => opt.Ignore());

        CreateMap<UpdateCategorySectionDto, CategorySection>()
            .ForMember(x => x.Icon, opt => opt.Ignore());
    }
}