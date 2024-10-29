using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IBlog
    {
        public APIResponse getList(BlogRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse create(BlogRequest request, string username);
        public APIResponse update(BlogRequest request, string username);
        public APIResponse delete(DeleteGuidRequest req);
    }
}
