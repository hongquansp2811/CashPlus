using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IAccumulatePointConfig
    {
        public APIResponse getList(AccumulatePointConfigRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse getDetailGeneral();
        public APIResponse create(AccumulatePointConfigRequest request, string username);
        public APIResponse update(AccumulatePointConfigRequest request, string username);
        public APIResponse delete(DeleteGuidRequest request);
    }
}
