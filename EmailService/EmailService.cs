using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportScheduler.EmailService
{
    
    public interface IEmailService
    {
        void Send( string to, string subject, string html);
        void Send( string to, string subject, string html,byte[] file);
    }

    public class EmailService : IEmailService
    {
        ILogger<EmailService> _logger;
        private readonly SmtpSettings _smtpSettings;
        private readonly IWebHostEnvironment _env;

        public EmailService(IOptions< SmtpSettings> smtpSettings,IWebHostEnvironment webHostEnvironment, ILogger<EmailService> logger)
        {
            _smtpSettings = smtpSettings.Value;
            _env = webHostEnvironment;
            _logger = logger;
        }

        public void Send(string to, string subject, string html)
        {
            try
            {
                // create message
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;
                email.Body = new TextPart(TextFormat.Html) { Text = html };

                using (var client = new SmtpClient())
                {
                    //client.ServerCertificateValidationCallback
                    if (_env.IsDevelopment())
                    {
                        client.Connect(_smtpSettings.Server, _smtpSettings.Port, true);
                    }
                    else
                    {
                        client.Connect(_smtpSettings.Server, _smtpSettings.Port);
                    }
                    client.Authenticate(_smtpSettings.Username, _smtpSettings.Password);
                    client.Send(email);
                    client.Disconnect(true);
                }
            }catch(Exception error)
            {
                _logger.LogInformation(error.Message);
            }
        }

        public void Send(string to, string subject, string text, byte[] file)
        {
            try
            {
                // create message
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail));
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;
                //email.Body = new TextPart(TextFormat.Html) { Text = html };
                var builder = new BodyBuilder();
                //builder.HtmlBody = htmlContent;
                builder.TextBody = text;
                builder.Attachments.Add("report.csv", file);
                email.Body = builder.ToMessageBody();
                using (var client = new SmtpClient())
                {
                    //client.ServerCertificateValidationCallback
                    if (_env.IsDevelopment())
                    {
                        client.Connect(_smtpSettings.Server, _smtpSettings.Port, true);
                    }
                    else
                    {
                        client.Connect(_smtpSettings.Server, _smtpSettings.Port);
                    }
                    client.Authenticate(_smtpSettings.Username, _smtpSettings.Password);
                    client.Send(email);
                    client.Disconnect(true);
                }
            }
            catch (Exception error)
            {
                _logger.LogInformation(error.Message);
            }
        }
    }

}
