using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IBank
    {
        public APIResponse getList(BankRequest request);
        public APIResponse getDetail(int id);
        public APIResponse create(BankRequest request, string username);
        public APIResponse update(BankRequest request, string username);
        public APIResponse delete(DeleteRequest id);
    }
}
