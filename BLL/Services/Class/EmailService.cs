using BLL.Services.Interface;
using DAL.Data.AuthModel;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TicketPluse.Helper;

public class EmailService : IEmailService
{
    private readonly Melseting _melseting;
    private readonly UserManager<App_user> _userManager;

    public EmailService(
        IOptions<Melseting> options,
        UserManager<App_user> userManager)
    {
        _melseting = options.Value;
        _userManager = userManager;
    }

    // 1. ميثود تأكيد الإيميل المكتوبة عندك
    public async Task<string> SendEmailAsync(
     string emailTo,
     string token,
     string controllerName,
     string reqUrl,
     string subject)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(emailTo);

            if (user is null)
                return "Email is incorrect.";

            var confirmationLink =
                $"{reqUrl}/{controllerName}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(_melseting.displayname, _melseting.Email));
            email.To.Add(MailboxAddress.Parse(emailTo));
            email.Subject = subject;
            email.Sender = MailboxAddress.Parse(_melseting.Email);

            var builder = new BodyBuilder();

            builder.HtmlBody = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
</head>

<body style=""margin:0;padding:30px;background:#f4f4f4;font-family:Arial,Helvetica,sans-serif;"">

    <div style=""
        max-width:600px;
        margin:auto;
        background:#ffffff;
        border-radius:10px;
        overflow:hidden;
        box-shadow:0 3px 10px rgba(0,0,0,.15);"">

        <div style=""
            background:#0d6efd;
            color:white;
            padding:20px;
            text-align:center;"">

            <h2 style=""margin:0;"">
                TicketPluse
            </h2>

        </div>

        <div style=""padding:35px;"">

            <h3 style=""color:#333;"">
                Confirm Your Email
            </h3>

            <p style=""font-size:15px;color:#555;line-height:1.8;"">
                Hello <strong>{user.UserName}</strong>,
            </p>

            <p style=""font-size:15px;color:#555;line-height:1.8;"">
                Thank you for registering with
                <strong>TicketPluse</strong>.
                Please confirm your email address by clicking the button below.
            </p>

            <div style=""text-align:center;margin:35px 0;"">

                <a href=""{confirmationLink}""
                   style=""
                   background:#0d6efd;
                   color:#fff;
                   padding:14px 28px;
                   text-decoration:none;
                   border-radius:6px;
                   font-weight:bold;"">

                    Confirm Email

                </a>

            </div>

            <p style=""font-size:14px;color:#777;"">
                If you didn't create this account, you can safely ignore this email.
            </p>

            <hr style=""border:none;border-top:1px solid #ddd;margin:30px 0;"" />

            <p style=""font-size:12px;color:#999;text-align:center;"">
                © 2026 TicketPluse. All Rights Reserved.
            </p>

        </div>

    </div>

</body>
</html>";

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _melseting.host,
                int.Parse(_melseting.port),
                MailKit.Security.SecureSocketOptions.Auto);

            await smtp.AuthenticateAsync(
                _melseting.Email,
                _melseting.password);

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);

            return string.Empty;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
    public async Task<string> SendResetPasswordEmailAsync(
      string emailTo,
      string token,
      string controllerName,
      string reqUrl,
      string subject)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(emailTo);

            if (user is null)
                return "Email is incorrect.";

            var resetLink =
                $"{reqUrl}/reset-password?userId={user.Id}&token={Uri.EscapeDataString(token)}";

            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(_melseting.displayname, _melseting.Email));
            email.To.Add(MailboxAddress.Parse(emailTo));
            email.Subject = subject;
            email.Sender = MailboxAddress.Parse(_melseting.Email);

            var builder = new BodyBuilder();

            builder.HtmlBody = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
</head>

<body style=""margin:0;padding:30px;background:#f4f4f4;font-family:Arial,Helvetica,sans-serif;"">

    <div style=""
        max-width:600px;
        margin:auto;
        background:#ffffff;
        border-radius:10px;
        overflow:hidden;
        box-shadow:0 3px 10px rgba(0,0,0,.15);"">

        <div style=""
            background:#dc3545;
            color:white;
            padding:20px;
            text-align:center;"">

            <h2 style=""margin:0;"">
                TicketPluse
            </h2>

        </div>

        <div style=""padding:35px;"">

            <h3 style=""color:#333;"">
                Reset Your Password
            </h3>

            <p style=""font-size:15px;color:#555;line-height:1.8;"">
                Hello <strong>{user.UserName}</strong>,
            </p>

            <p style=""font-size:15px;color:#555;line-height:1.8;"">
                We received a request to reset the password for your
                <strong>TicketPluse</strong> account.
            </p>

            <p style=""font-size:15px;color:#555;line-height:1.8;"">
                Click the button below to create a new password.
            </p>

            <div style=""text-align:center;margin:35px 0;"">

                <a href=""{resetLink}""
                   style=""
                   background:#dc3545;
                   color:#ffffff;
                   padding:14px 28px;
                   text-decoration:none;
                   border-radius:6px;
                   font-weight:bold;"">

                    Reset Password

                </a>

            </div>

            <p style=""font-size:14px;color:#777;"">
                If you didn't request a password reset, you can safely ignore this email.
                Your password will remain unchanged.
            </p>

            <hr style=""border:none;border-top:1px solid #ddd;margin:30px 0;"">

            <p style=""font-size:12px;color:#999;text-align:center;"">
                © 2026 TicketPluse. All Rights Reserved.
            </p>

        </div>

    </div>

</body>
</html>";

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _melseting.host,
                int.Parse(_melseting.port),
                MailKit.Security.SecureSocketOptions.Auto);

            await smtp.AuthenticateAsync(
                _melseting.Email,
                _melseting.password);

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);

            return string.Empty;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    public async Task SendNewEventNotificationAsync(
     string emailTo,
     string eventTitle,
     string place,
     DateTime startDate)
    {
        try
        {
            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(_melseting.displayname, _melseting.Email));
            email.To.Add(MailboxAddress.Parse(emailTo));
            email.Subject = $"🎉 New Event Available: {eventTitle}";
            email.Sender = MailboxAddress.Parse(_melseting.Email);

            var builder = new BodyBuilder();

            builder.HtmlBody = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
</head>

<body style=""margin:0;padding:30px;background:#f4f4f4;font-family:Arial,Helvetica,sans-serif;"">

    <div style=""
        max-width:600px;
        margin:auto;
        background:#ffffff;
        border-radius:10px;
        overflow:hidden;
        box-shadow:0 3px 10px rgba(0,0,0,.15);"">

        <div style=""
            background:#198754;
            color:white;
            padding:20px;
            text-align:center;"">

            <h2 style=""margin:0;"">
                🎉 TicketPluse
            </h2>

        </div>

        <div style=""padding:35px;"">

            <h3 style=""color:#333;"">
                A New Event Is Waiting For You!
            </h3>

            <p style=""font-size:15px;color:#555;line-height:1.8;"">
                We're excited to let you know that a new event has just been added.
            </p>

            <table style=""width:100%;border-collapse:collapse;margin:25px 0;"">
                <tr>
                    <td style=""padding:10px;font-weight:bold;width:140px;"">
                        🎫 Event
                    </td>
                    <td style=""padding:10px;"">
                        {eventTitle}
                    </td>
                </tr>

                <tr style=""background:#f8f9fa;"">
                    <td style=""padding:10px;font-weight:bold;"">
                        📍 Location
                    </td>
                    <td style=""padding:10px;"">
                        {place}
                    </td>
                </tr>

                <tr>
                    <td style=""padding:10px;font-weight:bold;"">
                        📅 Date
                    </td>
                    <td style=""padding:10px;"">
                        {startDate:dddd, dd MMMM yyyy - hh:mm tt}
                    </td>
                </tr>
            </table>

            <div style=""text-align:center;margin:35px 0;"">

                <a href=""#""
                   style=""
                   background:#198754;
                   color:#ffffff;
                   padding:14px 28px;
                   text-decoration:none;
                   border-radius:6px;
                   font-weight:bold;"">

                    View Event

                </a>

            </div>

            <p style=""font-size:14px;color:#777;"">
                Hurry up! Seats may be limited. Book your ticket now and don't miss this event.
            </p>

            <hr style=""border:none;border-top:1px solid #ddd;margin:30px 0;"">

            <p style=""font-size:12px;color:#999;text-align:center;"">
                © 2026 TicketPluse. All Rights Reserved.
            </p>

        </div>

    </div>

</body>
</html>";

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _melseting.host,
                int.Parse(_melseting.port),
                MailKit.Security.SecureSocketOptions.Auto);

            await smtp.AuthenticateAsync(
                _melseting.Email,
                _melseting.password);

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

}