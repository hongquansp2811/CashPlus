using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IPartnerContract
    {
        public APIResponse getList(PartnerContractRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse create(PartnerContractRequest request, string username);
        public APIResponse update(PartnerContractRequest request, string username);
        public APIResponse delete(DeleteGuidRequest req);
        public void updateContractExpire();
    }
}
