using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IFunction
    {
        public APIResponse getList(FunctionRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse create(Function request, string username);
        public APIResponse update(Function request, string username);
        public APIResponse delete(DeleteGuidRequest req);
        public APIResponse getListFunctionPermission();
        public APIResponse getFunctionTree();
    }
}
