using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Services.Interface
{
    public interface IEmailService
    {


        Task<string> SendEmailAsync(string emailto, string token, string controlname, string Requrl, string subject);
        Task<string> SendResetPasswordEmailAsync(string emailTo, string token, string controllerName, string reqUrl, string Subject);
        Task SendNewEventNotificationAsync(string emailTo, string eventTitle, string place, DateTime startDate);

    }
}
