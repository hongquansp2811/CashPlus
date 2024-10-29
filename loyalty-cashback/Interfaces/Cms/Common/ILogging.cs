using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface ILogging
    {
        public APIResponse getListLogIn(FilterLoggingRequest req);
        public APIResponse getListAction(FilterLoggingRequest req);
        public APIResponse getListCallApi(FilterLoggingRequest req);
    }
}
