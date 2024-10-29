using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface IRecallPointOrder
    {
        public APIResponse getListBlackUser(UserRequest request);
        public APIResponse create(RecallPointOrderRequest request, string username);
    }
}
