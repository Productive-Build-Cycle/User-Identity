using FluentValidation;
using FluentValidation.AspNetCore;
using Identity.Core.Data;
using Identity.Core.Exceptions;
using Identity.Core.MappingProfiles;
using Identity.Core.Options;
using Identity.Core.ServiceContracts;
using Identity.Core.Services;
using Identity.Core.Validators.AuthValidators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Identity.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {

        // TODO: Add JWT Service

        services.AddAutoMapper(typeof(UserProfile).Assembly);
        services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        services.Configure<JwtTokenOptions>(configuration.GetSection(nameof(JwtTokenOptions)));

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserService, UserrService>();
        services.AddScoped<IRolesService, RolesService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IdentityTranslatedErrors>();

        var connectionStrring = configuration.GetConnectionString("Default")
            ?? throw new KeyNotFoundException("CONNECTION STRING IS NULL!");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionStrring);
        });

        services.AddHostedService<RoleInitializer>();

        return services;

    }
}
