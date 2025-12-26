using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.DTOs.Users
{
    public sealed record UserDto(
     Guid Id,
     string Email,
     string? FirstName,
     string? LastName
 );
}
