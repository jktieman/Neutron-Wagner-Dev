using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using AlliedLogger;
using AlliedPostOffice.Concrete;

namespace SAPServer
{
    public class SendEmail
    {
        private readonly EmailProcessor _emailProcessor;
        private readonly List<string> _people;
        private readonly IDynamicLogger _logger;

        public SendEmail(List<string> people, IDynamicLogger logger)
        {
            _people = people;
            _logger = logger;
            _emailProcessor = new EmailProcessor();
        }
        public void StartUp()
        {
            var subject = "SAP Console Startup";
            var sb = _logger.LastLogLines();
            sb.AppendLine();
            sb.AppendLine("SAP Console has just started processing.");
            var body = sb;
            var attachments = new string[] { _logger.TempFilePath };
            _emailProcessor.ProcessEmail(subject, body, _people, attachments
                , new MailAddressCollection(), new MailAddressCollection());

        }

        public void ShutDown()
        {
            var subject = "SAP Console Normal ShutDown";
            var body = new StringBuilder("SAP Console has shut down normally.");
            var attachments = new string[] { _logger.CurrentLog };
            _emailProcessor.ProcessEmail(subject, body, _people, attachments
                , new MailAddressCollection(), new MailAddressCollection());

        }

        public void Message(string subject, StringBuilder body)
        {
            var attachments = new string[] { _logger.CurrentLog };
            _emailProcessor.ProcessEmail(subject, body, _people
                , attachments, new MailAddressCollection(), new MailAddressCollection());

        }
    }
}
