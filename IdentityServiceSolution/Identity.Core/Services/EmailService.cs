using Identity.Core.Options;
using Identity.Core.ServiceContracts;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Identity.Core.Services;

public class EmailService(IWebHostEnvironment env) : IEmailService
{
    public Task SendEmailAsync(EmailOptions options)
    {
        var address = "amirmahditeymoori123@gmail.com";
        var message = new MailMessage()
        {
            From = new MailAddress(address, "PBC - Identity"),
            To = { options.To },
            Subject = options.Subject,
            Body = options.Body,
            IsBodyHtml = true
        };

        var client = new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential(address, "abak aape zjrg shvr"),
            EnableSsl = true
        };

        client.Send(message);
        return Task.CompletedTask;
    }

    public async Task<string> TurnHtmlToString(string fileName, IDictionary<string, string> values)
    {
        var path = Path.Combine(
           env.ContentRootPath,
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
