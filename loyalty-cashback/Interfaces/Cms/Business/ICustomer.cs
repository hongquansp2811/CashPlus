using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface ICustomer
    {
        public APIResponse getList(CustomerRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse update(CustomerRequest request, string username);
        public APIResponse changePassword(PasswordRequest req);
        public APIResponse changeStatus(DeleteGuidRequest req);
        public APIResponse getListAccumulatePointOrder(AccumulatePointOrderRequest request);
        public APIResponse getListChangePointOrder(ChangePointOrderRequest request);
        public APIResponse getListPartnerOrder(PartnerOrderRequest request);
        public APIResponse getListAccumulatePointOrderRating(AccumulatePointOrderRatingRequest request);
        public APIResponse getListTeam(CustomerRequest request);
    }
}
