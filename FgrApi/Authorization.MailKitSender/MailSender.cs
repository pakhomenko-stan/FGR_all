using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace Common.MailKit
{
    public static class MailSender
    {
        public static Task<string> SendMail(this MimeMessage message, string host, int port)
        {
            SmtpClient smtpClient = new();
            smtpClient.Connect(host, port);
            var result = smtpClient.SendAsync(message);
            smtpClient.Dispose();   
            return result;
        }

        public static void AddTextBody(this MimeMessage message, string bodyText)
        {
            var body = new TextPart(TextFormat.Html)
            {
                Text = bodyText
            };
            message.Body = body;
        }

        public static void AddMimeTo(this MimeMessage message, string adr) => message.To.Add(new MailboxAddress(string.Empty, adr));

        public static void AddMimeBcc(this MimeMessage message, string adr) => message.Bcc.Add(new MailboxAddress(string.Empty, adr));

        public static MimeMessage GetMimeMessage(string adrFrom, string subj) => new()
        {
            Sender = new MailboxAddress(string.Empty, adrFrom),
            Subject = subj
        };
    }
}
