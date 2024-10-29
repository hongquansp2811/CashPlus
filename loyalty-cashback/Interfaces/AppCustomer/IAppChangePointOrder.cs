using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IAppChangePointOrder
    {
        public APIResponse getList(ChangePointOrderRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse create(ChangePointOrderRequest request, string username);
        public APIResponse cancelChangePoint(ChangePointOrderRequest request, string username);
        public APIResponse getExchangePack();
    }
}
