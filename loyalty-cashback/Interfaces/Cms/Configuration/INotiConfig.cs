using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface INotiConfig
    {
        public APIResponse getDetail();
        public APIResponse update(NotiConfigReq request, string username);
        public APIResponse getListHistory(ConfigHistoryRequest request);
    }
}
