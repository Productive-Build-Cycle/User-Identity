using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.DTOs.Auth
{
    public sealed record LoginRequestDto(
          string Email,
          string Password
        );
}
