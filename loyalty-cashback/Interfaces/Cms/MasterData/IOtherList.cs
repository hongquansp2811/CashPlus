using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IOtherList
    {
        public APIResponse getList(OtherListRequest request);
        public APIResponse getDetail(int id);
        public APIResponse create(OtherListRequest request, string username);
        public APIResponse update(OtherListRequest request, string username);
        public APIResponse delete(DeleteRequest req);
    }
}
