using Microsoft.Extensions.DependencyInjection;
using SavaloApp.Application.Abstracts.Repositories.CategorySections;
using SavaloApp.Application.Abstracts.Repositories.GoalSections;
using SavaloApp.Application.Abstracts.Repositories.Icons;
using SavaloApp.Application.Abstracts.Repositories.OtpCodes;
using SavaloApp.Application.Abstracts.Repositories.RefreshTokens;
using SavaloApp.Application.Abstracts.Repositories.TermsAndConditions;
using SavaloApp.Application.Abstracts.Services;
using SavaloApp.Infrastructure.Concretes.Services;
using SavaloApp.Persistance.Concretes.Repositories.CategorySections;
using SavaloApp.Persistance.Concretes.Repositories.GoalSections;
using SavaloApp.Persistance.Concretes.Repositories.Icons;
using SavaloApp.Persistance.Concretes.Repositories.OtpCodes;
using SavaloApp.Persistance.Concretes.Repositories.RefreshTokens;
using SavaloApp.Persistance.Concretes.Repositories.TermsAndConditions;
using SavaloApp.Persistance.Concretes.Services;

namespace SavaloApp.Persistance;

  public static class ServiceRegistration
    {
        public static void AddServices(this IServiceCollection services)
        {
            //Repositories
            //OtpCode
            services.AddScoped<IOtpCodeReadRepository, OtpCodeReadRepository>();
            services.AddScoped<IOtpCodeWriteRepository, OtpCodeWriteRepository>();
            
            //RefreshToken
            services.AddScoped<IRefreshTokenReadRepository, RefreshTokenReadRepository>();
            services.AddScoped<IRefreshTokenWriteRepository, RefreshTokenWriteRepository>();
            //CategorySection
            services.AddScoped<ICategorySectionReadRepository, CategorySectionReadRepository>();
            services.AddScoped<ICategorySectionWriteRepository, CategorySectionWriteRepository>();
            //GoalSection
            services.AddScoped<IGoalSectionReadRepository, GoalSectionReadRepository>();
            services.AddScoped<IGoalSectionWriteRepository, GoalSectionWriteRepository>();
            //Icons
            services.AddScoped<IIconReadRepository, IconReadRepository>();
            services.AddScoped<IIconWriteRepository, IconWriteRepository>();
            //TermsAndCondition
            services.AddScoped<ITermsAndConditionReadRepository, TermsAndConditionReadRepository>();
            services.AddScoped<ITermsAndConditionWriteRepository, TermsAndConditionWriteRepository>();
            
            
            
            //Services
            services.AddHttpClient<ISmsService, SmsService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddHttpClient<IAppleTokenValidator, AppleTokenValidator>();
            services.AddScoped<ITempTokenService, TempTokenService>();
            services.AddScoped<IUserAuthService, UserAuthService>();
            services.AddScoped<IFileService, CloudinaryFileService>();
            services.AddScoped<ICategorySectionService, CategorySectionService>();
            services.AddScoped<IAdminAuthService, AdminAuthService>();
            services.AddScoped<IGoalSectionService, GoalSectionService>();
            services.AddScoped<IIconService, IconService>();
            services.AddScoped<ITermsAndConditionService, TermsAndConditionService>();
           services.AddHttpClient<IOtpSenderService, OtpSenderService>();
            


        }
    }