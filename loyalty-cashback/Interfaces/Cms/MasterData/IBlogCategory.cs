using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IBlogCategory
    {
        public APIResponse getList(BlogCategoryRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse create(BlogCategoryRequest request, string username);
        public APIResponse update(BlogCategoryRequest request, string username);
        public APIResponse delete(DeleteGuidRequest req);
    }
}
