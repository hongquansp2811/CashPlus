using LOYALTY.Data;
using LOYALTY.Extensions;
using System.Threading.Tasks;
using LOYALTY.Models;
using System.Linq;
using LOYALTY.PaymentGate.Interface;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace LOYALTY.PaymentGate
{
    public class SendSMSBrandName : ISendSMSBrandName
    {

        private readonly LOYALTYContext _context;
        private readonly IEmailSender _emailSender;

        public SendSMSBrandName(LOYALTYContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        public async Task SendSMSBrandNameAsync(IServiceScopeFactory serviceScopeFactory, int? type, decimal Point, string phone, decimal Point_limit, string noti, string? limi, decimal Point_notSms)
        {
            string mess = noti.Replace("{TT_Diem_TD}", Point.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{TT_Tong_DTD}", Point_notSms.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{TT_Config_DTD}", Point_limit.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." }));

            if (type == 1) // type = 1 gửi điểm tích lũy
            {
                mess = noti.Replace("{TT_DiemTL}", Point.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{TT_Tong_DTL}", Point_notSms.ToString("N0", new NumberFormatInfo { NumberGroupSeparator = "." })).Replace("{TT_Config_DTD}", limi).Replace("{Config_Date_DTL}", limi);
               
            }
            _emailSender.SendSms(phone, mess);
        }
    }   
}
