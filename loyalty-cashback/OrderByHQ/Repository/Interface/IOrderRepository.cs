using LOYALTY.OrderByHQ.Models;
using Pig.AspNetCore.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.OrderByHQ.Repository.Interface
{
    public interface IOrderRepository : IBaseRepository<Order, Guid>
    {
        Task<List<Order>> GetOrdersByInvoiceIdAsync(Guid invoiceId);
    }
}
