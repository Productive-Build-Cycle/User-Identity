using FluentValidation;
using FluentValidation.AspNetCore;
using Identity.Core.Data;
using Identity.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Identity.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {

        // TODO: Add AutoMapper Profiles
        // TODO: Add ValidationClasses
        // TODO: Add JWT Service

        services.AddAutoMapper(typeof(ApplicationUser).Assembly);
        services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblyContaining<ApplicationUser>();

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
