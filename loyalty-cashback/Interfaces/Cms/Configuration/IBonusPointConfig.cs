using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IBonusPointConfig
    {
        public APIResponse getList(BonusPointConfigRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse create(BonusPointConfigRequest request, string username);
        public APIResponse update(BonusPointConfigRequest request, string username);
        public APIResponse delete(DeleteGuidRequest request);
    }
}
