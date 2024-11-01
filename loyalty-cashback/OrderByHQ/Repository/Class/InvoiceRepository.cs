using DocumentFormat.OpenXml.InkML;
using LOYALTY.OrderByHQ.Models;
using LOYALTY.OrderByHQ.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Pig.AspNetCore.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LOYALTY.OrderByHQ.Repository.Class
{
    public class InvoiceRepository : BaseRepository<Invoice, Guid> , IInvoiceRepository
    {
        public InvoiceRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
