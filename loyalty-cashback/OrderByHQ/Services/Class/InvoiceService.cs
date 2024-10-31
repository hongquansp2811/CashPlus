using AutoMapper;
using LOYALTY.OrderByHQ.Models;
using LOYALTY.OrderByHQ.Repository.Interface;
using LOYALTY.OrderByHQ.Services.Interface;
using System;
using System.Collections.Generic;

namespace LOYALTY.OrderByHQ.Services.Class
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IMapper _mapper;
        public InvoiceService(IInvoiceRepository invoiceRepository, IMapper mapper)
        {
            _invoiceRepository = invoiceRepository;
            _mapper = mapper;
        }

        public IEnumerable<invoice> GetList(Guid partner_id)
        {
            var invoices = _invoiceRepository.GetQueryable();
            return invoices;
        }
    }
}
