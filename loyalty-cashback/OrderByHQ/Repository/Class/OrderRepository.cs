using LOYALTY.OrderByHQ.Models;
using LOYALTY.OrderByHQ.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Pig.AspNetCore.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.OrderByHQ.Repository.Class
{
    public class OrderRepository : BaseRepository<Order, Guid>, IOrderRepository
    {
        public OrderRepository(DbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<Order>> GetOrdersByInvoiceIdAsync(Guid invoiceId)
        {
            return await GetQueryable().Where(x=>x.invoice_id.Equals(invoiceId)).ToListAsync();
        }
    }
}
