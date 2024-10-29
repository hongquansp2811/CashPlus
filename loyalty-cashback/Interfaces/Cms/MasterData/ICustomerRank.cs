using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface ICustomerRank
    {
        public APIResponse getList(CustomerRankRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse create(CustomerRankRequest request, string username);
        public APIResponse update(CustomerRankRequest request, string username);
        public APIResponse delete(DeleteGuidRequest req);
    }
}
