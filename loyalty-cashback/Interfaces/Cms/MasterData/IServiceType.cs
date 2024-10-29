using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IServiceType
    {
        public APIResponse getList(ServiceTypeRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse create(ServiceTypeRequest request, string username);
        public APIResponse update(ServiceTypeRequest request, string username);
        public APIResponse delete(DeleteGuidRequest req);
    }
}
