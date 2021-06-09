using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace AlliedPostOffice.Abstract
{
   public interface IEmailProcessor
    {
       void ProcessEmail(string subject, string body, List<string> people, Attachment attachment);
    }
}
