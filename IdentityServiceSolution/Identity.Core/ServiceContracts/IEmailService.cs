using Identity.Core.Dtos;

namespace Identity.Core.ServiceContracts;

public interface IEmailService
{
    Task SendEmailAsync(EmailMessageModel options);
    Task<string> TurnHtmlToString(string fileName, IDictionary<string, string> values); 
}
