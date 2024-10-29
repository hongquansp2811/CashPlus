using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IPartnerUser
    {
        public APIResponse getList(UserRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse create(UserRequest request, string username);
        public APIResponse update(UserRequest request, string username);
        public APIResponse delete(DeleteGuidRequest req);
        public APIResponse lockAccount(DeleteGuidRequest req);
        public APIResponse unlockAccount(DeleteGuidRequest req);
        public APIResponse changePass(DeleteGuidRequest req);
    }
}
