using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.DTOs.Users
{
    public sealed record CreateUserResultDto(
     Guid UserId,
     string Email
 );
}
