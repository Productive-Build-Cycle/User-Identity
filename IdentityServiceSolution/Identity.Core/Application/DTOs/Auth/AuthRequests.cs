using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.Application.DTOs.Auth
{
    public record RegisterRequest
    {
        public required string Email { get; init; }
        public required string UserName { get; init; }
        public required  string Password { get; init; }
        public required string ConfirmPassword { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required string PhoneNumber { get; init; }
    }
    
    public record LoginRequest
    {
        public required string Email { get; init; }
        public required string Password { get; init; }
    }
    
    public record UpdateUserRequest
    {
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? Email { get; init; }
        public string? PhoneNumber { get; init; }
    }

    public record ChangePasswordRequest
    {
        public required string CurrentPassword { get; init; }
        public required string NewPassword { get; init; }
        public required string ConfirmNewPassword { get; init; }
    }

    public record ForgotPasswordRequest
    {
        public required string Email { get; init; }
    }

    public record ResetPasswordRequest
    {
        public required string Token { get; init; }
        public required string NewPassword { get; init; }
        public required string ConfirmNewPassword { get; init; }
    }
}
