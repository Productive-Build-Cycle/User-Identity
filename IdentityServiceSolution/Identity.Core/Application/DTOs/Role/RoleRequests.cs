using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.Application.DTOs.Role
{
    public record AddRoleRequest
    {
        public required string Name { get; init; }
    }

    public record UpdateRoleRequest
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
    }
 
}
