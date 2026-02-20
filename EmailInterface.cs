using MailKit.Net.Smtp;
using MimeKit;

namespace MorrisvilleDiscordBot
{
    internal static class EmailInterface
    {
        public static bool SendVerificationEmail(string emailTo, string verificationID)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Morrisville Discord Server Verification", Program.config.EmailFrom));
            message.To.Add(new MailboxAddress("", emailTo));
            message.Subject = $"Your Morrisville Discord server verification code is: {verificationID}";
            message.Body = new TextPart("plain")
            {
                Text = $"To verify your account, please use the following code: {verificationID}"
            };

            using (var client = new SmtpClient())
            {
                try
                {
                    client.Connect(Program.config.SmtpHost, Program.config.SmtpPort, true);
                    client.Authenticate(Program.config.AccountUsername, Program.config.AccountPassword);
                    client.Send(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
                finally
                {
                    client.Disconnect(true);
                }
            }
            return true;
        }
    }
}
