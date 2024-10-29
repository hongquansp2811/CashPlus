using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IAccumulatePointOrderComplain
    {
        public APIResponse getList(AccumulatePointOrderComplainRequest request);
        public APIResponse create(AccumulatePointOrderComplainRequest request, string username);
        public APIResponse updateStatus(AccumulatePointOrderComplainRequest request, string username);
    }
}
