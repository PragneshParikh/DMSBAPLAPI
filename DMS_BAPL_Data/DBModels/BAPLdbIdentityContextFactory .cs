using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.DBModels
{
    public class BAPLdbIdentityContextFactory : IDesignTimeDbContextFactory<BAPLdbIdentityContext>
    {
        public BAPLdbIdentityContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BAPLdbIdentityContext>();

            optionsBuilder.UseSqlServer(
                "Server=bapldmsvad01.database.windows.net;Database=BAPLDMSvad;User Id=bapladmin;Password=$@plDMS_v@d1205;TrustServerCertificate=True;"
            );

            return new BAPLdbIdentityContext(optionsBuilder.Options);
        }
    }
}
