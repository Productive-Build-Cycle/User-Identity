using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.DTOs.Auth
{
    public sealed record AuthTokensResponseDto
    (
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAtUtc
);
}
