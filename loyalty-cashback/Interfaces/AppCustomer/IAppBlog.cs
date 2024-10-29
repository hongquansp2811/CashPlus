using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IAppBlog
    {
        public APIResponse getListCategory();
        public APIResponse getListBlog(BlogRequest request);
        public APIResponse getDetailBlog(Guid id);

    }
}
