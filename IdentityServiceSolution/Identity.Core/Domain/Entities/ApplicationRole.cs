using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.Domain.Entities;

public class ApplicationRole : IdentityRole<Guid> 
{ 
    public string Description { get; set; } = string.Empty;
}
