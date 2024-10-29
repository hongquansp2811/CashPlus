using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IOffending_Words
    {
        public APIResponse getList(Offending_WordsReq request);
        public APIResponse getDetail(Guid id);
        public APIResponse create(Offending_WordsReq request, string username);
        public APIResponse update(Offending_WordsReq request, string username);
        public APIResponse delete(DeleteGuidRequest req);
    }
}
