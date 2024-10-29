using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IPartner
    {
        public APIResponse getList(PartnerRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse getBalance(Guid partner_id);
        public APIResponse create(PartnerRequest request, string username);
        public APIResponse update(PartnerRequest request, string username);
        public APIResponse updateInStore(PartnerRequest request, string username);
        public APIResponse getListAccumulatePointOrder(AccumulatePointOrderRequest request);
        public APIResponse getListChangePointOrder(ChangePointOrderRequest request);
        public APIResponse getListPartnerOrder(PartnerOrderRequest request);
        public APIResponse getListAccumulatePointOrderRating(AccumulatePointOrderRatingRequest request);
        public APIResponse getListAddPointOrder(AddPointOrderRequest request);
        public APIResponse getListTeam(PartnerRequest request);
        public APIResponse delete(DeleteGuidRequest req);
        public APIResponse lockAccount(DeleteGuidRequest req);
        public APIResponse unlockAccount(DeleteGuidRequest req);
        public APIResponse changePassword(PasswordRequest req);
    }
}
