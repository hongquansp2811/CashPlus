using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IComplainInfo
    {
        public APIResponse getList(ComplainInfoRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse create(ComplainInfoRequest request, string username);
        public APIResponse update(ComplainInfoRequest request, string username);
        public APIResponse delete(DeleteGuidRequest request);
    }
}
