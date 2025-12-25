using Identity.Core.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Identity.Core.Data;

public class RoleInitializer : IHostedService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<RoleInitializer> _logger;

    public RoleInitializer(IServiceProvider serviceProvider, ILogger<RoleInitializer> logger)
    {
        _roleManager = serviceProvider.CreateScope()
            .ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ROLE INITIALIZER STARTED");

        var roles = new[] { "Admin", "Mentor", "User" };

        foreach (var roleName in roles)
        {
            try
            {
                if (await _roleManager.RoleExistsAsync(roleName))
                {
                    _logger.LogInformation("{Role} already exists, skipping...", roleName);
                    continue;
                }

                var result = await _roleManager.CreateAsync(
                    new ApplicationRole { Name = roleName });

                if (result.Succeeded)
                {
                    _logger.LogInformation("{Role} created successfully.", roleName);
                }
                else
                {
                    _logger.LogError(
                        "Failed to create {Role}: {Errors}",
                        roleName,
                        string.Join(", ", result.Errors.Select(e => e.Description))
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Exception while creating role {Role}",
                    roleName
                );
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
