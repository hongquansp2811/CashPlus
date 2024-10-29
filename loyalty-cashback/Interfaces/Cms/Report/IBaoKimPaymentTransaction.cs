using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IBaoKimPaymentTransaction
    {
        public APIResponse adminPaymentReport(ReportRequest request);
        public APIResponse partnerPaymentReport(ReportRequest request);
        public APIResponse partnerMoneyControlReport(ReportRequest request);
        public APIResponse partnerMoneyControlDetail(ReportRequest request);
        public APIResponse partnerMoneyControlPayment(ReportRequest request);
    }
}
