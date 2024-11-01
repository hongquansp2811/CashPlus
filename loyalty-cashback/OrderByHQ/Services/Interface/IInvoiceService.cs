using LOYALTY.OrderByHQ.DTO;
using System.Threading.Tasks;
using System;

namespace LOYALTY.OrderByHQ.Services.Interface
{
    public interface IInvoiceService
    {
        Task<InvoiceDetailDTO> GetInvoiceDetailAsync(Guid invoiceId);
    }
}
