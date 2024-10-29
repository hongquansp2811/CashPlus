using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IProductLabel
    {
        public APIResponse getList(ProductLabelRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse create(ProductLabelRequest request, string username);
        public APIResponse update(ProductLabelRequest request, string username);
        public APIResponse delete(DeleteGuidRequest req);
    }
}
