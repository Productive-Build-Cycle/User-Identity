using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.Domain.Entities;


public class ApplicationRole : IdentityRole<Guid> 
{ 
    public string Description { get; set; } = string.Empty;

}
public static class SystemRoles
{
    public const string Admin = "admin";
    public const string Mentor = "mentor";
    public const string User = "user";

    public static readonly HashSet<string> Allowed =
        new(StringComparer.OrdinalIgnoreCase)
        {
            Admin,
            Mentor,
            User
        };
}
