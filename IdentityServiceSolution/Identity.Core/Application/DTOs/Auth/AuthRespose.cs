using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.Application.DTOs.Auth
{
    public record AuthRespose
    {
        public required string AccessToken { get; init; }
        public required string RefreshToken { get; init; }
        public required DateTime AccessTokenExpiresAt { get; init; }

        public required UserInfoResponse User { get; init; }
        public record UserInfoResponse
        {
            public required string Id { get; init; }
            public required string Email { get; init; }
            public required string UserName { get; init; }

            public IReadOnlyList<string> Roles { get; init; } = [];
        }
    }
}
   
