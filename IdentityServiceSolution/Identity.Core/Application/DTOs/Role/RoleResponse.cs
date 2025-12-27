using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.Application.DTOs.Role
{
    public record RoleResponse
    {
        public required string Id { get; init; }
        public required string Name { get; init; }
    }
}
