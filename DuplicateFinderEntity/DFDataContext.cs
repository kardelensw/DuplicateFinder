using DuplicateFinderDAL;
using DuplicateFinderDAL.Migrations;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFinderDAL
{
    public class DFDataContext : DbContext
    {
        public DbSet<DFHash> DFHashes { get; set; }
        public DbSet<DFPath> DFPaths { get; set; }

        public DFDataContext() : base("DbCon")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<DFDataContext, Configuration>());
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DFPath>().Property(x => x.Id).HasPrecision(16, 0);
            modelBuilder.Entity<DFHash>().Property(x => x.Id).HasPrecision(16, 0);
        }

    }
}
