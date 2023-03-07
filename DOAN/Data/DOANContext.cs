using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DOAN.Models;

namespace DOAN.Data
{
    public class DOANContext : DbContext
    {
        public DOANContext (DbContextOptions<DOANContext> options)
            : base(options)
        {
        }

        public DbSet<DOAN.Models.testModel> testModel { get; set; } = default!;
        public DbSet<DOAN.Models.User> User { get; set; }
    }
}
