using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.DTOs.Auth
{
    public sealed record RegisterRequestDto(
         string Email,
         string Password,
         string FirstName,
         string LastName
        );
}
