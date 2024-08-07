using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Common.MailKit;

namespace Authorization.SSO.Services
{
    public class EmailService : IEmailSender
    {
        private readonly ServerOptions options;
        private readonly ILogger<EmailService> logger;

        public EmailService(
            IOptions<ServerOptions> options,
            ILogger<EmailService> logger)
        {
            this.options = options.Value;
            this.logger = logger;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (options.UseSmtp)
            {
                await SendEmail(subject, htmlMessage, email);
            }
        }

        public async Task SendEmail(string subject, string body, string recipient, string[] bcc = null, List<string> attachmentsPath = null)
        {
            if (string.IsNullOrEmpty(recipient))
                return;

            try
            {
                MimeMessage mail =  MailSender.GetMimeMessage(options.EmailFrom, subject);

                foreach (var address in recipient.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!string.IsNullOrEmpty(address))
                    {
                        try
                        {
                            mail.AddMimeTo(address);
                        }
                        catch (Exception e)
                        {
                            logger.LogError(string.Format("Error set recipient: {0}.  {1}. StackTrace: {2}", address, e.Message, e.StackTrace));
                        }
                    }
                }

                if (bcc != null)
                {
                    foreach (var address in bcc)
                    {
                        if (!string.IsNullOrEmpty(address))
                        {
                            try
                            {
                                mail.AddMimeBcc(address);
                            }
                            catch (Exception e)
                            {
                                logger.LogError(string.Format("Error set bcc: {0}.  {1}. StackTrace: {2}", address, e.Message, e.StackTrace));
                            }
                        }
                    }
                }
                mail.AddTextBody(body);
                await mail.SendMail(options.SmtpServer, options.SmtpPort);
            }
            catch (Exception e)
            {
                logger.LogError(string.Format("Error sending email: {0}. StackTrace: {1}", e.Message, e.StackTrace));
            }
        }

    }
}
