using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IAppPartnerOrder
    {
        public APIResponse getList(PartnerOrderRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse create(PartnerOrderRequest id, string username);
        public APIResponse getListProductGroup(CategoryRequest request);
        public APIResponse getListPartner(CategoryRequest request);
        public APIResponse getListPartnerTest(PartnerMapRequest request);

    }
}
