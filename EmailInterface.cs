using MailKit.Net.Smtp;
using MimeKit;

namespace MorrisvilleDiscordBot
{
    internal static class EmailInterface
    {
        public static bool SendVerificationEmail(string emailTo, string serverName, string verificationID)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress(null, Program.config.EmailFrom));
                    message.To.Add(new MailboxAddress("", emailTo));
                    message.Subject = $"Verify Email Address for {serverName}";
                    message.Body = new TextPart("plain")
                    {
                        Text = $"Hello! We need to make sure you're a part of this community! Please use the following code to verify your address: {verificationID}"
                    };

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
