using FluentValidation;
using FluentValidation.AspNetCore;
using Identity.Core.Data;
using Identity.Core.MappingProfiles;
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
