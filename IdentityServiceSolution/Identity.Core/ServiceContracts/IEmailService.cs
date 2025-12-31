using Identity.Core.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.ServiceContracts;

public interface IEmailService
{
    Task SendEmailAsync(EmailOptions options);
    Task<string> TurnHtmlToString(string fileName, IDictionary<string, string> values); 
}
