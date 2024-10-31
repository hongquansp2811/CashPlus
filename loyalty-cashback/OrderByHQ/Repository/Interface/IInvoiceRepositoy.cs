

using LOYALTY.OrderByHQ.Models;
using Pig.AspNetCore.Application.Repositories;
using System;

namespace LOYALTY.OrderByHQ.Repository.Interface
{
    public interface IInvoiceRepository : IBaseRepository<invoice, Guid>
    {
    }
}
