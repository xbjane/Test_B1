using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Test_B1
{
    internal class AppContext : DbContext
    {
        public AppContext() : base("DefaultConnection")
        {

        }
        public DbSet<StringData>? StringDatas { get; set; }
    }
}
