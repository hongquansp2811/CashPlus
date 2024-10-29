using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IAction
    {
        public APIResponse getList(ActionRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse create(Action1 request, string username);
        public APIResponse update(Action1 request, string username);
        public APIResponse delete(DeleteGuidRequest req);
    }
}
