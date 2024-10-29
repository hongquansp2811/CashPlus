using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LOYALTY.Models;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace LOYALTY.DataContext
{
    public class LOYALTYDapperContext : DbContext
    {
        public LOYALTYDapperContext() { }
        public LOYALTYDapperContext(DbContextOptions<LOYALTYDapperContext> options) : base(options) { }
    }
}
