using FluentResults;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Identity.Core.Exceptions;

public static class Errors
{
    public static Error DuplicateEmail(string email) =>
        new Error($"ایمیل '{email}' قبلاً ثبت شده است")
            .WithMetadata("StatusCode", HttpStatusCode.Conflict);

    public static Error UserNotFound(string userId) =>
        new Error($"کاربری با شناسه '{userId}' یافت نشد")
            .WithMetadata("StatusCode", HttpStatusCode.NotFound);

    public static Error InvalidToken =>
        new Error("توکن نامعتبر، منقضی یا قبلاً استفاده شده است")
            .WithMetadata("StatusCode", HttpStatusCode.BadRequest);

    public static Error EmailNotConfirmed =>
        new Error("لطفا حساب کاربری خود را تایید کنید")
            .WithMetadata("StatusCode", HttpStatusCode.Forbidden);

    public static Error AccountBanned =>
        new Error("اکانت شما مسدود شده است. برای اطلاعات بیشتر با پشتیبانی تماس بگیرید")
            .WithMetadata("StatusCode", HttpStatusCode.Forbidden);

    public static Error AccountLocked(TimeSpan remaining) =>
        new Error($"دسترسی شما به اکانت به مدت {remaining.Minutes} دقیقه محدود شده است")
            .WithMetadata("StatusCode", HttpStatusCode.Locked);

    public static Error InvalidCredentials =>
        new Error("رمز عبور یا نام کاربری نامعتبر است")
            .WithMetadata("StatusCode", HttpStatusCode.Unauthorized);

    public static Error Unauthorized =>
        new Error("شما اجازه انجام این عملیات را ندارید")
            .WithMetadata("StatusCode", HttpStatusCode.Forbidden);

    public static Error AlreadyBanned =>
        new Error("کاربر مورد نظر درحال حاضر مسدود شده است")
            .WithMetadata("StatusCode", HttpStatusCode.Conflict);

    public static Error NotBanned =>
        new Error("کاربر مورد نظر درحال حاضر مسدود نشده")
            .WithMetadata("StatusCode", HttpStatusCode.Conflict);
}