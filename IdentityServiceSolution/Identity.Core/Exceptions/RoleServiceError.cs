using FluentResults;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Identity.Core.Exceptions
{
    public static class RoleErrors
    {
        public static Error RoleNotFound(string RoleName) =>
            new Error($"نقش  '{RoleName}' یافت نشد.")
                .WithMetadata(ErrorMetadataKeys.StatusCode, HttpStatusCode.NotFound);
        public static Error RoleNotFound(Guid Id) =>
            new Error($"نقش با شناسه '{Id}' یافت نشد.")
                .WithMetadata(ErrorMetadataKeys.StatusCode, HttpStatusCode.Conflict);
        public static Error DuplicateRole(Guid Id) =>
            new Error($"نقش با شناسه '{Id}' قبلاً ثبت شده است")
                .WithMetadata(ErrorMetadataKeys.StatusCode, HttpStatusCode.Conflict);
        public static Error InvalidRoleName(string roleName) =>
            new Error($"نقش '{roleName}' مجاز نیست. فقط admin, mentor, user قابل استفاده هستند.")
                .WithMetadata("StatusCode", HttpStatusCode.BadRequest);
        public static Error RoleHasAssignedUsers(Guid roleId) =>
            new Error($"امکان حذف نقش '{roleId}' وجود ندارد زیرا کاربر به آن تخصیص داده شده است.")
                .WithMetadata(ErrorMetadataKeys.StatusCode, HttpStatusCode.Conflict);
        public static Error RoleHasClaims(Guid roleId) =>
            new Error($"نقش '{roleId}' دارای Claim فعال است و قابل حذف نیست.")
                .WithMetadata(ErrorMetadataKeys.StatusCode, HttpStatusCode.Conflict);
        public static Error ClaimAlreadyExistsForRole(Guid roleId, string type, string value) =>
            new Error($"Claim '{type}:{value}' قبلاً به نقش '{roleId}' تخصیص داده شده است.")
                .WithMetadata(ErrorMetadataKeys.StatusCode, HttpStatusCode.Conflict);
        public static Error ClaimNotFoundForRole(Guid roleId, string type, string value) =>
            new Error($"Claim '{type}:{value}' برای نقش '{roleId}'  یافت نشد..")
                .WithMetadata(ErrorMetadataKeys.StatusCode, HttpStatusCode.Conflict);
    };

    public static class ErrorMetadataKeys
    {
        public const string StatusCode = "StatusCode";
    }

}
