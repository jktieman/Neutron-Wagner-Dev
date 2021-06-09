using System.Net.Mail;

namespace AlliedPostOffice.Concrete
{
    public class EmailSettings
    {
        public EmailSettings()
        {
            MailReplyTo = new MailAddressCollection();
        }

        public bool IsBodyHtml { get; set; }

        public string DefaultReplyToAddress { get; set; }

        public int ServerPort { get; set; }

        public MailAddressCollection MailReplyTo { get; set; }

        public string MailCc { get; set; }

        public string FileLocation { get; set; }

        public bool WriteAsFile { get; set; }

        public string ServerName { get; set; }

        public string Password { get; set; }

        public string Username { get; set; }

        public bool UseSsl { get; set; }

        public string MailFromAddress { get; set; }

        public string MailToAddress { get; set; }

        
    }
}
