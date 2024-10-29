using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IAffiliateConfig
    {
        public APIResponse getList(AffiliateConfigRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse getDetailGeneral();
        public APIResponse create(AffiliateConfigRequest request, string username);
        public APIResponse update(AffiliateConfigRequest request, string username);
        public APIResponse delete(DeleteGuidRequest request);
    }
}
