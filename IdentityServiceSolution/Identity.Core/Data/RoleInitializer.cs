using System.Security.Claims;
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
        var scope = serviceProvider.CreateScope();
        _roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ROLE INITIALIZER STARTED");

        var rolePermissions = new Dictionary<string, string[]>
        {
            ["Admin"] = new[]
            {
                "user.create","user.update","user.delete","user.ban","user.unban",
                "role.create","role.update","role.delete"
            },
            ["Mentor"] = new[] { "user.ban", "user.unban", "user.update", "user.delete" },
            ["User"] = new[] { "user.delete", "user.update" }
        };

        foreach (var (roleName, permissions) in rolePermissions)
        {
            try
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var createResult = await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                    if (!createResult.Succeeded)
                    {
                        _logger.LogError(
                            "Failed to create {Role}: {Errors}",
                            roleName,
                            string.Join(", ", createResult.Errors.Select(e => e.Description))
                        );
                        continue;
                    }
                    _logger.LogInformation("{Role} created successfully.", roleName);
                }

                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null) continue;

                var existingClaims = await _roleManager.GetClaimsAsync(role);

                foreach (var permission in permissions)
                {
                    if (existingClaims.Any(c => c.Type == "permission" && c.Value == permission))
                        continue;

                    var claimResult = await _roleManager.AddClaimAsync(role, new Claim("permission", permission));

                    if (claimResult.Succeeded)
                        _logger.LogInformation("Added permission {Permission} to role {Role}", permission, roleName);
                    else
                        _logger.LogError("Failed to add permission {Permission} to role {Role}", permission, roleName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while initializing role {Role}", roleName);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
