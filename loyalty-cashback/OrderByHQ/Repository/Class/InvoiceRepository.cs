using LOYALTY.OrderByHQ.Models;
using LOYALTY.OrderByHQ.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using Pig.AspNetCore.Application.Repositories;
using System;

namespace LOYALTY.OrderByHQ.Repository.Class
{
    public class InvoiceRepository : BaseRepository<invoice, Guid> , IInvoiceRepository
    {
        public InvoiceRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
