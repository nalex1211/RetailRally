using MimeKit;
using MailKit.Net.Smtp;

namespace RetailRally.Helpers;

public class EmailService(IConfiguration _configuration)
{
    public async Task SendEmailAsync(string toEmail, string subject, string url, string bodyText, string btnText)
    {
        var emailMessage = new MimeMessage();
        string fromEmail = _configuration["EmailConfig:EmailFrom"];
        string hostPassword = _configuration["EmailConfig:HostPasswrod"];

        emailMessage.From.Add(new MailboxAddress("RetailRally", fromEmail)); 
        emailMessage.To.Add(new MailboxAddress("", toEmail));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart("html")
        {
            Text = @$"<html>
                <body>
                    <p>{bodyText}</p>
                    <a href='{url}' style='padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px;'>Change {btnText}</a>
                </body>
             </html>"
        };


        using (var client = new SmtpClient())
        {
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync("smtp.gmail.com", 465, true); 
            await client.AuthenticateAsync(fromEmail, hostPassword);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}
