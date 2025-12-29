using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? RefreshToken { get; set; }
    public int LockoutMultiplier { get; set; } = 1;
    public bool Banned { get; set; } = false;
    public DateTime RefreshTokenExpiery { get; set; }
}
