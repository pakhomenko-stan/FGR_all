using System;
using System.Threading.Tasks;
using Common.MailKit;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Authorization.SSO.Services
{
    public class EmailService(
        IOptions<ServerOptions> options,
        ILogger<EmailService> logger) : IEmailSender
    {
        private readonly ServerOptions options = options.Value;
        private readonly ILogger<EmailService> logger = logger;
        private static readonly string[] separator = [";"];

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (options.UseSmtp)
            {
                await SendEmail(subject, htmlMessage, email);
            }
        }

        public async Task SendEmail(string subject, string body, string recipient, string[] bcc = null)
        {
            if (string.IsNullOrEmpty(recipient))
                return;

            try
            {
                MimeMessage mail = MailSender.GetMimeMessage(options.EmailFrom, subject);

                foreach (var address in recipient.Split(separator, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!string.IsNullOrEmpty(address))
                    {
                        try
                        {
                            mail.AddMimeTo(address);
                        }
                        catch (Exception e)
                        {
                            logger.LogError("Error set recipient: {Address}.  {Message}. StackTrace: {Trace}", address, e.Message, e.StackTrace);
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
                                logger.LogError("Error set bcc: {Address}.  {Message}. StackTrace: {Trace}", address, e.Message, e.StackTrace);
                            }
                        }
                    }
                }
                mail.AddTextBody(body);
                await mail.SendMail(options.SmtpServer, options.SmtpPort);
            }
            catch (Exception e)
            {
                logger.LogError("Error sending email: {Message}. StackTrace: {Trace}", e.Message, e.StackTrace);
            }
        }

    }
}
