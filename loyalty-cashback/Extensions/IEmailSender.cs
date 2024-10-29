using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.Models;

namespace LOYALTY.Extensions
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
        Task SendListEmailAsync(List<string> emails, string subject, string message);
        Task SendSms(string phone_number, string message);
        Task SendEmailAsyncTempalte(string email, string subject, string message);

    }
}
