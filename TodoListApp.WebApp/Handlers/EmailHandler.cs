using System.Net;
using System.Net.Mail;

namespace TodoListApp.WebApp.Services.Email;

public class EmailHandler
{
    private readonly IConfiguration _config;

    public EmailHandler(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var host = _config["MailSettings:Host"];
        var port = int.Parse(_config["MailSettings:Port"]!);
        var user = _config["MailSettings:UserName"];
        var pass = _config["MailSettings:Password"];

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(user, pass),
            EnableSsl = true
        };

        var mail = new MailMessage
        {
            From = new MailAddress("no-reply@todoapp.com", "Todo App"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mail.To.Add(to);

        await client.SendMailAsync(mail);
    }
}
