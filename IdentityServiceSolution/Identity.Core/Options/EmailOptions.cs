using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.Options;

public class EmailOptions
{
    public EmailOptions(string to, string subject, string body)
    {
        To = to;
        Subject = subject;
        Body = body;
    }
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}
