using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IAppSuggestSearch
    {
        public APIResponse getList(AppSuggestSearchRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse create(AppSuggestSearchRequest request, string username);
        public APIResponse update(AppSuggestSearchRequest request, string username);
        public APIResponse delete(DeleteGuidRequest request);
    }
}
