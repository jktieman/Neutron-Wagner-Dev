using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AlliedPostOffice.Abstract;
using System.Net.Mail;
using System.Net;
using System.Windows.Forms;
using AlliedLogger;

namespace AlliedPostOffice.Concrete
{
    public class EmailProcessor : IEmailProcessor
    {
        private readonly EmailSettings _emailSettings;
        private IDynamicLogger _logger;

        public EmailProcessor()
        {
            Init();
        }
        public EmailProcessor(EmailSettings emailSettings)
        {
            _emailSettings = emailSettings;
            Init();
        }
        // initialize with settings
        private void Init()
        {
            _logger = NeutronCore.Global.Logger.SetupLogger("EmailProcessor");
        }

        public void ProcessEmail(string subject, string body, List<string> people, Attachment attachment = null)
        {

            using (var smtpClient = new SmtpClient())
            {
                smtpClient.EnableSsl = _emailSettings.UseSsl;
                smtpClient.Host = _emailSettings.ServerName;
                smtpClient.Port = _emailSettings.ServerPort;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
                if (_emailSettings.WriteAsFile)
                {
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    smtpClient.PickupDirectoryLocation = _emailSettings.FileLocation;
                    smtpClient.EnableSsl = false;
                }


                if (people.Count > 0)
                {
                    foreach (string email in people)
                    {

                        var mailMessage = new MailMessage(
                            _emailSettings.MailFromAddress
                            , email
                            , subject
                            , body)
                        { IsBodyHtml = _emailSettings.IsBodyHtml };


                        //Reply to Supervisor
                        if (_emailSettings.MailReplyTo.Count > 0)
                        {
                            foreach (MailAddress reply in _emailSettings.MailReplyTo)
                            {
                                mailMessage.ReplyToList.Add(reply);
                            }
                        }


                        if (_emailSettings.MailCc != String.Empty)
                        {
                            mailMessage.CC.Add(_emailSettings.MailCc);
                        }

                        if (attachment != null)
                        {
                            mailMessage.Attachments.Add(attachment);
                        }


                        //if (attachments != null)
                        //{
                        //    // Add the attachments
                        //    foreach (string attachment in attachments)
                        //    {
                        //        if (File.Exists(attachment))
                        //        {
                        //            Attachment a = new Attachment(attachment, "application/vnd.ms-excel");
                        //            mailMessage.Attachments.Add(a);

                        //        }
                        //    }
                        //}
                        try
                        {

                            if (_emailSettings.WriteAsFile)
                            {
                                MessageBox.Show($"Email written to file: {_emailSettings.FileLocation}" +
                                                $"{Environment.NewLine}{mailMessage.Body}");
                             _ = _logger.LogDetailAsync($"Email written to file: {_emailSettings.FileLocation}{Environment.NewLine}{mailMessage.Body}");
                                mailMessage.BodyEncoding = Encoding.ASCII;
                                File.WriteAllText(_emailSettings.FileLocation, mailMessage.Body);
                            }
                            else
                            {
                                smtpClient.Send(mailMessage);
                            }
                        }
                        catch (SmtpException ex)
                        {
                         _ = _logger.LogDetailAsync($"Error Sending Email: {ex.Message}");
                            throw new SmtpException($"SMTP Error Sending Email: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                         _ = _logger.LogDetailAsync($"Error Sending Email: {ex.Message}");
                            throw new Exception($"Error Sending Email: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}
