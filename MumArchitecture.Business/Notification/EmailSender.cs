using MumArchitecture.Business.Logging;
using MumArchitecture.Business.Result;
using MumArchitecture.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Notification
{
    public class EmailSender
    {
        private readonly string smtpServer="";
        private readonly string smtpMail = "";
        private readonly int    smtpPort = 587;
        private readonly string senderEmail = "";
        private readonly string senderPassword = "";


        public EmailSender()
        {
            smtpServer = AppSettings.instance?.EmailSMTP?.SmtpServer ?? "";
            smtpMail = AppSettings.instance?.EmailSMTP?.SmtpMail ?? "";
            smtpPort = AppSettings.instance?.EmailSMTP?.SmtpPort??587;
            senderEmail = AppSettings.instance?.EmailSMTP?.From ?? "";
            senderPassword = AppSettings.instance?.EmailSMTP?.SmtpPassword ?? "";
        }
        public EmailSender(SmtpSettings settings)
        {
            smtpMail = settings.SmtpMail ?? "";
            smtpServer = settings.SmtpServer ?? "";
            senderEmail = settings.SenderEmail ?? "";
            senderPassword = settings.SenderPassword ?? "";
            smtpPort = settings.SmtpPort;
        }


        public SystemResult<bool> SendMail(MailInfo mail)
        {
            var result = new SystemResult<bool>();
            try
            {
                MailMessage message = new MailMessage();
                message.From = string.IsNullOrWhiteSpace(mail.From)?new MailAddress(senderEmail) : new MailAddress(mail.From);
                message.To.Add(new MailAddress(mail.Email));
                message.Subject = mail.Subject;
                message.Body = mail.Message;
                message.BodyEncoding = Encoding.UTF32;
                message.SubjectEncoding = Encoding.UTF32;
                message.IsBodyHtml = mail.IsHtml;

                using (SmtpClient client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.Credentials = new NetworkCredential(smtpMail, senderPassword);
                    client.EnableSsl = true;
                    client.Send(message);
                }
                result.Data = true;
            }
            catch (Exception ex)
            {
                result.Data = false;
                result.AddMessage(ex.Message, EPriority.SystemError);
                LogManager.LogException(ex);
            }
            return result;
        }

    }

    public class MailInfo
    {
        public required string Email { get; set; }
        public string? Subject { get; set; }
        public required string Message { get; set; }
        public bool IsHtml { get; set; } = true;
        public string? From { get; set; }
    }
    public class SmtpSettings
    {
        public string? SmtpServer { get; set; } = string.Empty;
        public string? SmtpMail { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string? SenderEmail { get; set; } = string.Empty;
        public string? SenderPassword { get; set; } = string.Empty;
    }

}
