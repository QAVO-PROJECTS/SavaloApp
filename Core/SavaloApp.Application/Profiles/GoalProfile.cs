using AutoMapper;
using SavaloApp.Application.Dtos.Goal;
using SavaloApp.Domain.Entities;
using System.Globalization;

namespace SavaloApp.Application.Profiles;

public class GoalProfile : Profile
{
    public GoalProfile()
    {
        // ✅ ENTITY → DTO
        CreateMap<Goal, GoalDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.CurrencyAccountId, opt => opt.MapFrom(src => src.CurrencyAccountId.ToString()))
            .ForMember(dest => dest.IconId, opt => opt.MapFrom(src => src.IconId.HasValue ? src.IconId.ToString() : null))
            .ForMember(dest => dest.GoalSectionId, opt => opt.MapFrom(src => src.GoalSectionId.HasValue ? src.GoalSectionId.ToString() : null))
            .ForMember(dest => dest.GoalSectionName, opt => opt.MapFrom(src => src.GoalSection != null ? src.GoalSection.Name : null))
            .ForMember(dest => dest.GoalSectionImage, opt => opt.MapFrom(src => src.GoalSection != null ? src.GoalSection.Icon : null))
            .ForMember(dest => dest.StartDate,
                opt => opt.MapFrom(src => src.StartDate.HasValue
                    ? src.StartDate.Value.ToString("yyyy-MM-dd")
                    : null))
            .ForMember(dest => dest.EndDate,
                opt => opt.MapFrom(src => src.EndDate.HasValue
                    ? src.EndDate.Value.ToString("yyyy-MM-dd")
                    : null))
            .ForMember(dest => dest.TotalTransactionAmount,
                opt => opt.MapFrom(src => src.GoalTransactions != null
                    ? src.GoalTransactions.Sum(x => x.TransactionType ? -x.Amount : x.Amount)
                    : 0));

        // ✅ CREATE DTO → ENTITY
        CreateMap<CreateGoalDto, Goal>()
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
                        return DateTime.SpecifyKind(date, DateTimeKind.Utc);
                    }

                    return (DateTime?)null;
                }))
            .ForMember(dest => dest.EndDate,
                opt => opt.MapFrom((src, dest) =>
                {
                    if (string.IsNullOrWhiteSpace(src.EndDate))
                        return (DateTime?)null;

                    if (DateTime.TryParseExact(
                            src.EndDate,
                            "dd.MM.yyyy",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out var date))
                    {
                        return DateTime.SpecifyKind(date, DateTimeKind.Utc);
                    }

                    return (DateTime?)null;
                }));

        // ✅ UPDATE DTO → ENTITY
        CreateMap<UpdateGoalDto, Goal>()
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
                        return DateTime.SpecifyKind(date, DateTimeKind.Utc);
                    }

                    return (DateTime?)null;
                }))
            .ForMember(dest => dest.EndDate,
                opt => opt.MapFrom((src, dest) =>
                {
                    if (string.IsNullOrWhiteSpace(src.EndDate))
                        return (DateTime?)null;

                    if (DateTime.TryParseExact(
                            src.EndDate,
                            "dd.MM.yyyy",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out var date))
                    {
                        return DateTime.SpecifyKind(date, DateTimeKind.Utc);
                    }

                    return (DateTime?)null;
                }))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}