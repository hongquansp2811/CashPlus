using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IUserGroup
    {
        public APIResponse getList(UserGroupRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse create(UserGroupRequest request, string username);
        public APIResponse update(UserGroupRequest request, string username);
        public APIResponse delete(DeleteGuidRequest req);
        public APIResponse getPermission(Guid id);
    }
}
