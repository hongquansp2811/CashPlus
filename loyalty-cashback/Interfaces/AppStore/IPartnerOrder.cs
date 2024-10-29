using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IPartnerOrder
    {
        public APIResponse getList(PartnerOrderRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse confirm(Guid id, string username);
    }
}
