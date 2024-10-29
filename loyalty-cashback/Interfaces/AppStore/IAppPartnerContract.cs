using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IAppPartnerContract
    {
        public APIResponse getDetailPartner(Guid partner_id);
        public APIResponse getDetailPartnerContract(Guid partner_id);
    }
}
