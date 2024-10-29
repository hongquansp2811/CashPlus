using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IStaticPage
    {
        public APIResponse getList(StaticPageRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse getDetailByCode(string code);
        public APIResponse create(StaticPageRequest request, string username);
        public APIResponse update(StaticPageRequest request, string username);
        public APIResponse delete(DeleteGuidRequest req);
    }
}
