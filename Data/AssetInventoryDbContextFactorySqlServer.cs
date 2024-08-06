using Microsoft.EntityFrameworkCore.Design;
using System.Data.Common;

namespace AssetInventory.Data
{
	public class AssetInventoryDbContextFactorySqlServer : IDesignTimeDbContextFactory<AssetInventoryDbContextSqlServer>
	{
		public AssetInventoryDbContextSqlServer CreateDbContext(string[] args)
		{
			var database = @"(localdb)\MSSQLLocalDB";
			Console.WriteLine($"Using {database} as design time database.");
			var dbcsb = new DbConnectionStringBuilder()
			{
				{ "Data Source", database },
				{ "Initial Catalog", "AssetInventory" },
				{ "Integrated Security", true },
				{ "Connect Timeout", 30 },
				{ "Encrypt", false },
				{ "Trust Server Certificate", false },
				{ "Application Intent", "ReadWrite" },
				{ "Multi Subnet Failover", false },
			};
			return new() { ConnectionString_SqlServer = dbcsb.ConnectionString };
		}
	}

	public class AssetInventoryDbContextFactorySqlite : IDesignTimeDbContextFactory<AssetInventoryDbContextSqlite>
	{
		public AssetInventoryDbContextSqlite CreateDbContext(string[] args)
		{
			var file = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "AssetInventory.db");
			Console.WriteLine($"Using {file} as design time database.");
			return new AssetInventoryDbContextSqlite()
			{
				ConnectionString_Sqlite = new DbConnectionStringBuilder()
				{
					{ "Data Source" , file }
				}.ConnectionString
			};
		}
	}
}