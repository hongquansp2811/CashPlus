using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IReportCMS
    {
        public APIResponse addPointControlReport(ReportRequest request);
        public APIResponse accumulatePointControlReport(ReportRequest request);
        public APIResponse changePointControlReport(ReportRequest request);
        public APIResponse revenueReport(ReportRequest request);
    }
}
