using LOYALTY.OrderByHQ.Models;
using Pig.AspNetCore.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LOYALTY.OrderByHQ.Repository.Interface
{
    public interface IOrderItemRepository : IBaseRepository<OrderItem, Guid>
    {
        Task<List<OrderItem>> GetOrderItemsByOrderIdAsync(Guid orderId);

    }
}
