using Identity.Core.Options;
using Identity.Core.ServiceContracts;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Net.Mail;
using System.Text;
using Identity.Core.Dtos;
using Microsoft.Extensions.Options;

namespace Identity.Core.Services;

public class EmailService : IEmailService
{
    private readonly IWebHostEnvironment _env;
    private readonly MailCredits _mailCreds;
    public EmailService(IWebHostEnvironment env, IOptionsMonitor<MailCredits> mailCreds)
    {
        _env = env;
        _mailCreds = mailCreds.CurrentValue;
    }
    public Task SendEmailAsync(EmailMessageModel options)
    {
        var message = new MailMessage()
        {
            From = new MailAddress(_mailCreds.MailAddress, _mailCreds.MailTitle),
            To = { options.To },
            Subject = options.Subject,
            Body = options.Body,
            IsBodyHtml = true
        };

        var client = new SmtpClient(_mailCreds.StmpServer, _mailCreds.StmpPort)
        {
            Credentials = new NetworkCredential(_mailCreds.MailAddress, _mailCreds.StmpPassword),
            EnableSsl = true
        };

        client.Send(message);
        return Task.CompletedTask;
    }

    public async Task<string> TurnHtmlToString(string fileName, IDictionary<string, string> values)
    {
        var path = Path.Combine(
           _env.ContentRootPath,
           "EmailTemplates",
           fileName
       );

        var html = await File.ReadAllTextAsync(path, Encoding.UTF8);

        foreach (var item in values)
        {
            html = html.Replace($"{{{{{item.Key}}}}}", item.Value);
        }

        return html;
    }
}
