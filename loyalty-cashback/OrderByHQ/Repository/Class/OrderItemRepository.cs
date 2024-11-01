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
    public class OrderItemRepository : BaseRepository<OrderItem, Guid>, IOrderItemRepository
    {
        public OrderItemRepository(DbContext dbContext) : base(dbContext)
        {
        }

        public async Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(Guid orderId)
        {
            return await GetQueryable().Where(x=>x.order_id.Equals(orderId)).ToListAsync();
        }
    }
}
