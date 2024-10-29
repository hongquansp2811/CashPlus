using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IProvince
    {
        public APIResponse getList(ProvinceRequest request);
        public APIResponse getDetail(int id);
        public APIResponse create(ProvinceRequest request, string username);
        public APIResponse update(ProvinceRequest request, string username);
        public APIResponse delete(DeleteRequest req);
    }
}
