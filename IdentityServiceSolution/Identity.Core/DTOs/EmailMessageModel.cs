using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.Dtos;

public class EmailMessageModel
{
    public EmailMessageModel(string to, string subject, string body)
    {
        To = to;
        Subject = subject;
        Body = body;
    }
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}