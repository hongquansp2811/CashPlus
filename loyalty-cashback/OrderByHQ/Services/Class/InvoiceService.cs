using AutoMapper;
using LOYALTY.OrderByHQ.DTO;
using LOYALTY.OrderByHQ.Models;
using LOYALTY.OrderByHQ.Repository.Interface;
using LOYALTY.OrderByHQ.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LOYALTY.OrderByHQ.Services.Class
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IMapper _mapper;
        public InvoiceService(IInvoiceRepository invoiceRepository, IMapper mapper, IOrderRepository orderRepository, IOrderItemRepository orderItemRepository)
        {
            _invoiceRepository = invoiceRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _mapper = mapper;
        }

        public async Task<InvoiceDetailDTO> GetInvoiceDetailAsync(Guid invoiceId)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
            var orders = await _orderRepository.GetOrdersByInvoiceIdAsync(invoiceId);
            var invoiceDTO = _mapper.Map<InvoiceDetailDTO>(invoice);
           // invoiceDTO.OrderItems = orders
           //.SelectMany(order =>
           //    _orderItemRepository.GetOrderItemsByOrderIdAsync(order.id).Result
           //        .Select(orderItem => new OrderItemDetailDTO
           //        {
           //            ProductName = "", // Map from product if needed
           //            Quantity = orderItem.Quantity,
           //            BasePrice = orderItem.BasePrice,
           //            PriceOverride = orderItem.PriceOverride
           //        })
           //).ToList();

            return invoiceDTO;
        }
    }
}
