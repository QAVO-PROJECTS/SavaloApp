using System.Globalization;
using System.Text;
using FluentValidation.AspNetCore;

using SavaloApp.Domain.Entities;

using SavaloApp.Persistance;
using SavaloApp.Persistance.Context;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;


using Microsoft.AspNetCore.Localization;
using SavaloApp.Application.Profiles;
using SavaloApp.Application.Settings;
using SavaloApp.Application.Validations.Auth;
using SavaloApp.Domain.HelperEntities;
using SavaloApp.Persistance.Seeders;


var builder = WebApplication.CreateBuilder(args);



// FluentValidation
builder.Services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<RegisterRequestDtoValidator>())
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .SelectMany(kvp => kvp.Value.Errors.Select(e => e.ErrorMessage))
                .ToArray();

            var errorResponse = new
            {
                StatusCode = 400,
                Error = errors
            };

            return new BadRequestObjectResult(errorResponse);
        };
    });


builder.Services.AddAutoMapper(typeof(CategorySectionProfile).Assembly);

// DbContext
builder.Services.AddDbContext<SavaloAppDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default"));
});

// ===============================
//  Identity (User ⇒ Admin TPH)
// ===============================
builder.Services.AddIdentity<User, IdentityRole>(opt =>
{
    opt.Password.RequireNonAlphanumeric = false;
    opt.User.RequireUniqueEmail = false;
})
.AddEntityFrameworkStores<SavaloAppDbContext>()
.AddDefaultTokenProviders();

// ===============================
//  Authentication (JWT)
// ===============================
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"])
        ),
        LifetimeValidator = (_, expireDate, token, _) =>
            token != null ? expireDate > DateTime.UtcNow : false
    };
});

// ===============================
//   Authorization Policies
// ===============================


// Token Lifespan
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(24);
});

// Custom Services
builder.Services.AddServices();
builder.Services.Configure<SmsSettings>(
    builder.Configuration.GetSection("SmsSettings"));

// Mail
 builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

// Swagger
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo() { Title = "SAVOLO API", Version = "v1" });

    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter JWT",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[]{}
        }
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("corsapp", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.Configure<AppleAuthSettings>(
    builder.Configuration.GetSection("AppleAuth"));

builder.Services.AddMemoryCache();


var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}
var supportedCultures = new[] { new CultureInfo("en-US") };

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// builder.WebHost.UseUrls("http://0.0.0.0:5093");




// Swagger UI
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("corsapp");


app.UseAuthentication();  // VERY IMPORTANT
app.UseAuthorization();



app.MapControllers();



app.Run();