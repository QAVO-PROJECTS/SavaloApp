using System.Globalization;
using AutoMapper;
using SavaloApp.Application.Dtos.Category;
using SavaloApp.Domain.Entities;

namespace SavaloApp.Application.Profiles;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.CurrencyAccountId, opt => opt.MapFrom(src => src.CurrencyAccountId.ToString()))
            .ForMember(dest => dest.IconId, opt => opt.MapFrom(src => src.IconId.ToString()))
            .ForMember(dest => dest.CategorySectionImage,
                opt => opt.MapFrom(src => src.CategorySection != null ? src.CategorySection.Icon : null))

            .ForMember(dest => dest.RepeatType, opt => opt.MapFrom(src => src.Repeat))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.HasValue
                ? src.StartDate.Value.ToString("yyyy-MM-dd")
                : null))
;
        CreateMap<CreateCategoryDto, Category>()
            .ForMember(dest => dest.Repeat,
                opt => opt.MapFrom(src => src.RepeatType.ToString()))
            .ForMember(dest => dest.StartDate,
                opt => opt.MapFrom((src, dest) =>
                {
                    if (string.IsNullOrWhiteSpace(src.StartDate))
                        return (DateTime?)null;

                    if (DateTime.TryParseExact(
                            src.StartDate,
                            "dd.MM.yyyy",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out var date))
                    {
                        return DateTime.SpecifyKind(date, DateTimeKind.Utc); // 🔥 FIX
                    }

                    return (DateTime?)null;
                }));

        CreateMap<UpdateCategoryDto, Category>()
            .ForMember(dest => dest.Repeat, opt => opt.MapFrom(src => src.RepeatType!.ToString()))
            .ForMember(dest => dest.StartDate,
                opt => opt.MapFrom((src, dest) =>
                {
                    if (string.IsNullOrWhiteSpace(src.StartDate))
                        return (DateTime?)null;

                    if (DateTime.TryParseExact(
                            src.StartDate,
                            "dd.MM.yyyy",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out var date))
                    {
                        return DateTime.SpecifyKind(date, DateTimeKind.Utc); // 🔥 FIX
                    }

                    return (DateTime?)null;
                }))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}