using LOYALTY.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;


namespace LOYALTY.PaymentGate.Interface
{
    public interface ISendSMSBrandName
    {
        Task SendSMSBrandNameAsync(IServiceScopeFactory serviceScopeFactory, int? type, decimal Point, string phone, decimal Point_limit, string noti, string? config, decimal Point_notSms);   
    }
}
