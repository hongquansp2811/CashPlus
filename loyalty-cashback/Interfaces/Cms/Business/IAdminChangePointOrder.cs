using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IAdminChangePointOrder
    {
        public APIResponse getList(ChangePointOrderRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse approve(ChangePointOrderRequest request, string username);
    }
}
