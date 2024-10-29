using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IBanner
    {
        public APIResponse getList(BannerRequest request);
        public APIResponse getDetail(int id);
        public APIResponse create(BannerRequest request, string username);
        public APIResponse update(BannerRequest request, string username);
        public APIResponse delete(DeleteRequest req);
        public APIResponse changeStatus(DeleteRequest req);
    }
}
