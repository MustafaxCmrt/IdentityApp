using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace IdentityApp.Data;

public class SmtpEmailSender : IEmailSender, Interfaces.IEmailSender
{
    private string? _host;
    private int _port;
    private bool _enableSsl;
    private string? _username;
    private string? _password;

    public SmtpEmailSender(string? host, int port, bool enableSsl, string? username, string? password)
    {
        _host = host;
        _port = port;
        _enableSsl = enableSsl;
        _username = username;
        _password = password;
    }
    public Task SendEmailAsync(string email, string subject, string message)
    {
        var client = new SmtpClient(_host, _port)
        {
            Credentials = new NetworkCredential(_username, _password),
            EnableSsl = _enableSsl
        };
        return client.SendMailAsync(new MailMessage(_username ?? "", email, subject, message){IsBodyHtml = true});
    }
}