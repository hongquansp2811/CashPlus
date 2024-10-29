using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IBankAccount
    {
        public APIResponse getList(BankAccountRequest request);
        public APIResponse getDetail(Guid id);
        public APIResponse create(BankAccountRequest request, string username);
        public APIResponse update(BankAccountRequest request, string username);
        public APIResponse delete(DeleteGuidRequest request);
    }
}
