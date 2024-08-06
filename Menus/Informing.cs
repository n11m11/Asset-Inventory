using System.Data.Common;
using Asset_Inventory.ExtensionMethods;
using AssetInventory.Logic;
using AssetInventory.Models;
using AssetInventory.Printing;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace AssetInventory.Menus
{
    internal partial class _MenusClass
	{

		public void Info()
		{
			bool relational = ctx.Database.IsRelational();
			try
			{
				Console.WriteLine();
				Console.WriteLine("DbContext:");
				Console.WriteLine("  Test connection: " + ctx.Database.TestConnection(out _));
				Console.WriteLine("  Can migrate: " + ctx.Database.PreviouslyMigrated());
				Console.WriteLine("  ChangeTracker:");
				Console.WriteLine("    HasChanges: " + ctx.ChangeTracker.HasChanges());
				Console.WriteLine("    LazyLoadingEnabled: " + ctx.ChangeTracker.LazyLoadingEnabled);
				Console.WriteLine("  DatabaseFacade:");
				Console.WriteLine("    ProviderName: " + ctx.Database.ProviderName);
				Console.WriteLine("    CanConnect: " + ctx.Database.CanConnect());
				Console.WriteLine("    IsRelational: " + ctx.Database.IsRelational());
				Console.WriteLine("    IsSqlServer: " + ctx.Database.IsSqlServer());
				Console.WriteLine("    IsSqlite: " + ctx.Database.IsSqlite());
				Console.WriteLine("    IsInMemory: " + ctx.Database.IsInMemory());
				if (relational)
				{
					try
					{
						//System.InvalidOperationException: 'Metadata changes are not allowed when the model has been marked as read-only.'
						Console.WriteLine("    HasPendingModelChanges: " + ctx.Database.HasPendingModelChanges());
					}
					catch (InvalidOperationException)
					{
						Console.WriteLine("    HasPendingModelChanges: error");
					}
					Console.WriteLine("    GetPendingMigrations:");
					foreach (var x in ctx.Database.GetPendingMigrations().Append("[end of list]"))
						Console.WriteLine("      " + x);
					Console.WriteLine("    GetAppliedMigrations:");
					foreach (var x in ctx.Database.GetAppliedMigrations().Append("[end of list]"))
						Console.WriteLine("      " + x);
				}
				else
				{
					Console.WriteLine("    HasPendingModelChanges: relational databases only");
					Console.WriteLine("    GetAppliedMigrations:   relational databases only");
				}
				if (relational)
				{
					try
					{
						var dbcsb = new DbConnectionStringBuilder() { ConnectionString = ctx.Database.GetConnectionString() };
						Console.WriteLine("  ConnectionString:");
						foreach (string x in dbcsb.Keys)
							Console.WriteLine($"    {x} = {dbcsb[x]}");
						Console.WriteLine();
						Console.WriteLine(ctx.Database.GetConnectionString());
						Console.WriteLine();
					}
					catch
					{
					}
				}
			}
			catch (Exception ex)
			{
				AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything | ExceptionFormats.NoStackTrace);
			}
			Console.WriteLine();
			ConsolePrompt.Pause();
		}

		public void ShowStatistics()
		{
			var ratios = new BreakdownChart();
			foreach (AssetType type in new EnumEnumerable<AssetType>())
			{
				var count = ctx.Assets.AsNoTracking().Where(x => x.Type == type).Count();
				if (type != AssetType.Unknown || count > 0)
					ratios.AddItem(type + "",
								   count,
								   ColorHash.QuickHash(type + ""));
			}
			AnsiConsole.Write(new Padder(ratios));
			Console.WriteLine();
			ConsolePrompt.Pause();
		}
	}
}