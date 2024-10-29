using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IAppPartnerBag
    {
        public APIResponse getList(Guid customer_id);
        public APIResponse getDetailByPartnerId(PartnerBagRequest request);
        public APIResponse create(PartnerBagRequest request, string username);

    }
}
