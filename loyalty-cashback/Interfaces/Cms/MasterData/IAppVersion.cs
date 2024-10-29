using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IAppVersion
    {
        public APIResponse getList(AppVersionRequest request);
        public APIResponse getDetail(int id);
        public APIResponse create(AppVersionRequest request, string username);
        public APIResponse update(AppVersionRequest request, string username);
        public APIResponse delete(DeleteRequest req);
    }
}
