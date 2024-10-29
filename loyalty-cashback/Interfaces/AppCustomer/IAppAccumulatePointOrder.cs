using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IAppAccumulatePointOrder
    {
        public APIResponse getPartnerDetailByQR(Guid id);
        public APIResponse create(AccumulatePointOrderRequest id, string username);
        public Task<APIResponse> createPaymentLink(AccumulatePointOrderRequest request);
        public APIResponse updateOrder(AccumulatePointOrderRequest request);
        public APIResponse getList(AccumulatePointOrderRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse createRating(AccumulatePointOrderRatingRequest id, string username);
        public APIResponse QR(AccumulatePointOrderRequest request);
        public APIResponse getByTransNo(string? trans_no);
    }
}
