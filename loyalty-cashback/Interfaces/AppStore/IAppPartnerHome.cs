using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IAppPartnerHome
    {
        // API lấy thông tin trang chủ: Banner
        public APIResponse getHomeInfo(Guid partner_id, string username);
        public APIResponse generalInfo();
        public APIResponse getNotiNotRead(Guid partner_id);
        public APIResponse getCustomerFakeBank(Guid partner_id);
    }
}
