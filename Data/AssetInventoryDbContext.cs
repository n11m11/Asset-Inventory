using AssetInventory.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AssetInventory.Data
{
    public class AssetInventoryDbContext : DbContext
	{
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Office> Offices { get; set; }
        public DbSet<HttpCache> HttpCaches { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseLazyLoadingProxies();
#if DEBUG
			optionsBuilder.EnableSensitiveDataLogging();
#endif
		}
	}

    public class AssetInventoryDbContextSqlServer : AssetInventoryDbContext
    {
        public string? ConnectionString_SqlServer { get; set; } = null;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer(ConnectionString_SqlServer);
        }
    }

    public class AssetInventoryDbContextSqlite : AssetInventoryDbContext
    {
        public string? ConnectionString_Sqlite { get; set; } = null;
        public bool UseInMemory { get; set; } = false;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            if (UseInMemory)
            {
				SqliteConnection x = new SqliteConnection("Filename=:memory:");
                // the database only exists while a connection is open
                x.Open();
				optionsBuilder.UseSqlite(x);
            }
            else
            {
				optionsBuilder.UseSqlite(ConnectionString_Sqlite);
            }
        }
    }
    public class AssetInventoryDbContextInMemory : AssetInventoryDbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseInMemoryDatabase("AssetInventory");
            //optionsBuilder.ConfigureWarnings(wcb => wcb.Ignore(InMemoryEventId.TransactionIgnoredWarning));
		}
    }
}