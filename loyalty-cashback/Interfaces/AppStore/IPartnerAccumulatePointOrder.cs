using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IPartnerAccumulatePointOrder
    {
        public APIResponse getCustomerDetailByQR(Guid id, Guid partner_id);
        public APIResponse getList(AccumulatePointOrderRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse getListCMS(AccumulatePointOrderRequest request);
        public APIResponse getDetailCMS(Guid id);
        public APIResponse confirm(Guid id, string username);
        public APIResponse denied(Guid id, string username);
        public APIResponse create(AccumulatePointOrderRequest request, string username);

        public APIResponse CashPayment(Guid id, string username);

        public Task<APIResponse> createPaymentLink(AccumulatePointOrderRequest request);
        public Task<APIResponse> createPaymentLinkFull(AccumulatePointOrderRequest request);
        public APIResponse cashPaymentOnline(Guid id, string username);
    }
}
