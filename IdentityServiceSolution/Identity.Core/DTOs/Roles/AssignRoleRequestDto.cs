using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.DTOs.Roles
{
    public sealed record AssignRoleRequestDto(
    Guid UserId,
    string RoleName
);
}
