using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Core.Options;

public class MailCredits
{
    public string MailAddress { get; set; }
    public string MailTitle { get; set; }
    public string StmpServer { get; set; }

    public int StmpPort { get; set; }
    public string StmpPassword { get; set; }
}