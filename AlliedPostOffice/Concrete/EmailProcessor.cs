using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AlliedPostOffice.Abstract;
using System.Net.Mail;
using System.Net;

namespace AlliedPostOffice.Concrete
{
    public class EmailProcessor : IEmailProcessor
    {
        private readonly EmailSettings _emailSettings;

        public EmailProcessor(EmailSettings emailSettings)
        {
            _emailSettings = emailSettings;
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
                            , body) {IsBodyHtml = _emailSettings.IsBodyHtml};


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

                        if (_emailSettings.WriteAsFile)
                        {
                            mailMessage.BodyEncoding = Encoding.ASCII;
                        }

                        try
                        {
                            smtpClient.Send(mailMessage);
                            System.Threading.Thread.Sleep(500);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }
            }
        }
    }
}
