using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IAdminBaoKimTransaction
    {
        public APIResponse getListBKTransaction(AccumulatePointOrderRequest request);
        public APIResponse getListPartnerBK(AccumulatePointOrderRequest request);
        public APIResponse getDetailBKTransaction(Guid id);
        public APIResponse paymentCashPlus(AccumulatePointOrderRequest request);
    }
}
