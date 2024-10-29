using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IPartnerAddPointOrder
    {
        public APIResponse getList(AddPointOrderRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse getCustomerFakeBank(Guid user_id);
        public APIResponse getBonusPointDescription(Guid partner_id);
    }
}
