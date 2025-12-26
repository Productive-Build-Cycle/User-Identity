using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.Validators;

public static class CustomRequestValidator
{
    public static bool BeValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var validDomains = new[]
        {
            "gmail.com", "google.com", "outlook.com", "outlook.org", "hotmail.com",
            "yahoo.com", "yahoo.co.uk", "icloud.com", "aol.com", "protonmail.com",
            "zoho.com", "mail.com", "gmx.com", "live.com", "msn.com", "rediffmail.com",
            "inbox.com", "fastmail.com", "btinternet.com", "me.com", "mac.com"
        }; // برای جلوگیری از استفاده از سرویس های ایمیل فیک مثل temp mail و ...

        var parts = email.Split('@');
        if (parts.Length != 2)
            return false;

        var domain = parts[1].ToLowerInvariant();

        return validDomains.Contains(domain);
    }
}