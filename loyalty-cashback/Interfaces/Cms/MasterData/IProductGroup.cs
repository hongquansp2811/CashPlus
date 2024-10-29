using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IProductGroup
    {
        public APIResponse getList(ProductGroupRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse create(ProductGroupRequest request, string username);
        public APIResponse update(ProductGroupRequest request, string username);
        public APIResponse delete(DeleteGuidRequest req);
    }
}
