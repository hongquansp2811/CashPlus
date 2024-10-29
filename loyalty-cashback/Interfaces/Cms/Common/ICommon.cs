﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.DataObjects.Request;
using LOYALTY.DataObjects.Response;
using LOYALTY.Models;

namespace LOYALTY.Interfaces
{
    public interface ICommon
    {
        public bool checkUpdateCustomerRank(Guid user_id);
        //public Boolean createContract(CustomerContractRequest req);
    }
}