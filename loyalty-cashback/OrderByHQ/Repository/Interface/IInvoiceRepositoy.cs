﻿

using LOYALTY.OrderByHQ.Models;
using Pig.AspNetCore.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LOYALTY.OrderByHQ.Repository.Interface
{
    public interface IInvoiceRepository : IBaseRepository<Invoice, Guid>
    {
        
    }
}
