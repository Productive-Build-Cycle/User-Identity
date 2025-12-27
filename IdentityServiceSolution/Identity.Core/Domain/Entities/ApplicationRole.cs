using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.Domain.Entities;

public class ApplicationRole : BaseEntity
{

    public required string RoleName { get; set; }
}
