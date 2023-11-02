using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using AlliedPostOffice.Concrete;
using NeutronEvents;


namespace AlliedPostOffice
{
    public class SendEmail : ISendEmail
    {
        private readonly EmailProcessor _emailProcessor;
        private readonly List<string> _people;

        public SendEmail()
        {

        }

        public SendEmail(EmailProcessor emailProcessor, List<EmailAddressData> people)
        {
            _emailProcessor = emailProcessor;
            _people = people.Select(r => r.EmailAddress).ToList();
        }
        public void StartUp()
        {
            var subject = "Neutron Loader Startup";
            var body = $"Neutron Loader has just started processing.";
            var attachment = GetAttachment();
            try
            {
                _emailProcessor.ProcessEmail(subject, body, _people, attachment);
            }
            catch (Exception ex)
            {
                Mediator.GetInstance().OnGeneralError(this, $"{subject} {ex.Message}");

            }
        }

        public void ShutDown()
        {
            var subject = "Neutron Loader Normal ShutDown";
            var body = $"Neutron Loader has shut down normally.";
            var attachment = GetAttachment();
            try
            {
                _emailProcessor.ProcessEmail(subject, body, _people, attachment);
            }
            catch (Exception ex)
            {
                Mediator.GetInstance().OnGeneralError(this, $"{subject} {ex.Message}");
            }

        }

        public void StartUpUpload()
        {
            var subject = "Neutron Upload Startup";
            var body = $"Neutron Upload has just started processing.";
            var attachment = GetAttachment();
            try
            {
                _emailProcessor.ProcessEmail(subject, body, _people, attachment);
            }
            catch (Exception ex)
            {
                Mediator.GetInstance().OnGeneralError(this, $"{subject} {ex.Message}");
            }
        }

        public void ShutDownUpload()
        {
            var subject = "Neutron Upload Normal ShutDown";
            var body = $"Neutron Upload has shut down normally.";
            var attachment = GetAttachment();
            try
            {
                _emailProcessor.ProcessEmail(subject, body, _people, attachment);
            }
            catch (Exception ex)
            {
                Mediator.GetInstance().OnGeneralError(this, $"{subject} {ex.Message}");
            }

        }

        public void Message(string body, List<string> logLastLines)
        {
            var subject = "Neutron Loader Error";
            var lines = new List<string>();

            lines.Add($"{body}{Environment.NewLine}");
            foreach (var logLastLine in logLastLines)
            {
                lines.Add(logLastLine);
            }

            var messageBody = string.Join(Environment.NewLine, lines);
            var attachment = GetAttachment();
            try
            {
                _emailProcessor.ProcessEmail(subject, messageBody, _people, attachment);
            }
            catch (Exception ex)
            {
                Mediator.GetInstance().OnGeneralError(this, $"{subject} {ex.Message}");
            }

        }

        private Attachment GetAttachment()
        {
            return null;
            //var attachmentPath = _logger.TempFilePath;
            //return File.Exists(attachmentPath) ? new Attachment(attachmentPath, "application/vnd.ms-excel") : null;
        }

        public void StartUpSingleRun(List<string> logLastLines)
        {
            var subject = "Neutron Loader Single Run";
            var body = string.Join(Environment.NewLine, logLastLines);
            var attachment = GetAttachment();
            try
            {
                _emailProcessor.ProcessEmail(subject, body, _people, attachment);
            }
            catch (Exception ex)
            {
                Mediator.GetInstance().OnGeneralError(this, $"{subject} {ex.Message}");
            }
        }

        public void StartUpSingleRunUpload(List<string> logLastLines)
        {
            var subject = "Neutron Upload Single Run";
            var body = string.Join(Environment.NewLine, logLastLines);
            var attachment = GetAttachment();
            try
            {
                _emailProcessor.ProcessEmail(subject, body, _people, attachment);
            }
            catch (Exception ex)
            {
                Mediator.GetInstance().OnGeneralError(this, $"{subject} {ex.Message}");
            }
        }

        public void StartUpSap()
        {
            var subject = "SAP Console Startup";
            var body = "SAP Console has just started processing.";
            var attachment = GetAttachment();
            _emailProcessor.ProcessEmail(subject, body, _people, attachment);

        }

        public void ShutDownSap()
        {
            var subject = "SAP Console Normal ShutDown";
            var body = @"SAP Console has shut down normally.";
            var attachment = GetAttachment();
            _emailProcessor.ProcessEmail(subject, body, _people, attachment);
        }

        public void Message(string subject, StringBuilder body)
        {
            var attachment = GetAttachment();
            //var attachments = new string[] { _logger.CurrentLog };
            _emailProcessor.ProcessEmail(subject, body.ToString(), _people
                , attachment);

        }
    }
}
