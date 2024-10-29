using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IOtherListType
    {
        public APIResponse getList(OtherListTypeRequest request);
        public APIResponse getDetail(int id);
        public APIResponse create(OtherListType request, string username);
        public APIResponse update(OtherListType request, string username);
        public APIResponse delete(DeleteRequest req);
    }
}
