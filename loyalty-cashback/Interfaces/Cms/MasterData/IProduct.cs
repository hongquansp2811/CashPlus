using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IProduct
    {
        public APIResponse getList(ProductRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse create(ProductRequest request, string username);
        public APIResponse update(ProductRequest request, string username);
        public APIResponse delete(DeleteGuidRequest req, string username);
        public APIResponse sendApprove(DeleteGuidRequest req, string username);
        public APIResponse approve(DeleteGuidRequest req);
        public APIResponse denied(DeleteGuidRequest req);
        public APIResponse getListWeb(ProductRequest request);

    }
}
