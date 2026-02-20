using System.Net;
using System.Net.Mail;

namespace MorrisvilleDiscordBot
{
    internal static class EmailInterface
    {
        //Sends a verification email to the given Email Address
        //Returns whether the email was sent successfully.
        public static bool SendVerificationEmail(string emailAddress, string verificationID)
        {
            // Create the verification email.
            MailMessage message = new MailMessage(
                Program.config.EmailFrom,
                emailAddress,
                "Morrisville Discord Server Verification", //Subject
                $"To verify your account, please use the following code: {verificationID}"); //Contents

            //Configure the smtpClient
            SmtpClient client = new SmtpClient(Program.config.SmtpHost);
            client.Credentials = new NetworkCredential(Program.config.EmailUsername, Program.config.EmailPassword);
            client.EnableSsl = true;
            client.Port = Program.config.SmtpPort;

            //Attempt to send the email, log any errors if so
            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in CreateMessageWithAttachment(): {0}",
                    ex.ToString());
                return false;
            }
            return true;
        }



    }
}
