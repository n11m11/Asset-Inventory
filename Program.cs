using Asset_Inventory;
using Asset_Inventory.ExtensionMethods;
using Asset_Inventory.JsonConfig;
using AssetInventory.Data;
using AssetInventory.Menus;
using AssetInventory.Printing;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

using static AssetInventory.Menus._InteractiveSetup;

// TODO: exception handling.
// The database connection should be closed graciously for Sqlite.

namespace AssetInventory;

enum Restart
{
	no = 0, yes = 1
}

internal class AssetInventory
{
	static void Main(string[] args)
	{
		AnsiTools.EnableVT();
		AnsiTools.TrySetTitle("Asset Inventory");

		// We need a banner animation due to the long startup times
		IntroBanner banner = new();
		banner.Start();

		// our app has a configuration file, of course
		// see the README for more information.
		JsonRepository _early_config = new();
		AssetInventoryDbContext? _early_ctx_setup = null;
		bool _early_failed = false;
		try
		{
			_early_ctx_setup = _early_config.GetCtx();
#if DEBUG
			_early_ctx_setup ??= new AssetInventoryDbContextFactorySqlServer().CreateDbContext([]);
#endif
			// try to connect
			if (_early_ctx_setup is not null)
				if (!_early_ctx_setup.Database.TestConnection(out Exception? ex))
					_early_failed = true;
			if (_early_failed == false)
			{

			}
		}
		catch (Exception ex)
		{
			banner.Join();
			AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything | ExceptionFormats.NoStackTrace);
			_early_failed = true;
		}
		if (_early_failed)
		{
			_early_ctx_setup?.Dispose();
			_early_ctx_setup = null;
			banner.Join();
#if DEBUG
			//_early_config.DumpConfig();
#endif
			Console.WriteLine();
			Console.WriteLine("Connecting to the database automatically didn't work. Make sure that the");
			Console.WriteLine("  database exists then check your internet connection and connection string.");
		}

		if (_early_ctx_setup is null)
		{
			banner.Join();
			Console.WriteLine();
			Console.WriteLine("Thank you for using AssetInventory!");
			Console.WriteLine("To get started, we are going to have to connect you to a database.");
			Console.WriteLine();

			_early_ctx_setup = InteractiveSetupMenu();
			if (_early_ctx_setup is null)
				return;
		}


		Restart restart;
		using (AssetInventoryDbContext ctx = _early_ctx_setup)
		{
			banner.Join();
			restart = MainLoop(ctx, _early_config);
		}
		if (restart == Restart.yes)
			while (MainLoop(null, null) == Restart.yes) ;
		Console.WriteLine();
	}

	static Restart MainLoop(AssetInventoryDbContext? ctx, JsonRepository? config)
	{
		config ??= new();
		ctx ??= InteractiveSetupMenu();
		if (ctx is null)
			return global::AssetInventory.Restart.no;

		if (ctx.Database.PreviouslyMigrated())
		{
			var pending = ctx.Database.GetPendingMigrations();
			if (pending.Any())
			{
				var t = new Table().HideHeaders().AddColumn("");
				foreach (var s in pending)
					t.AddRow(s);
				AnsiConsole.Write(new Padder(new Rows(new Text("Available migrations"), t)));
				Console.WriteLine("Asset Inventory needs to apply migrations to work properly");
				Console.WriteLine("and continuing without migrating will result in data loss.");
				Console.WriteLine();
				if (AnsiConsole.Confirm("Apply migrations?"))
					ctx.Database.Migrate();
				Console.WriteLine();
			}
		}




		_MenusClass subMenus = new()
		{
			ctx = ctx,
			assetQueryChain = new(() => ctx.Assets.AsSplitQuery()),
			currencyApiKey = config.GetApiKey()
		};

		bool running = true;
		bool restartFlag = false;

		void DeleteDatabase()
		{
			if (subMenus._bool_try_to_DeleteDatabase())
			{
				running = false;
				restartFlag = true;
			}
		}

		void Exit()
		{
			ctx.SaveChangesWithStatusAndLog();
			running = false;
		}

		void restart()
		{
			ctx.SaveChangesWithStatusAndLog();
			running = false;
			restartFlag = true;
		}

		OrderedChoiceList mainMenu =
		[
			new(0, "seed",     "Insert sample assets for demo purposes")  { Callback = new ActionOrWriteException(subMenus.Seed )},
			new(0, "seedo",    "Insert sample offices for demo purposes") { Callback = new ActionOrWriteException(subMenus.SeedOffices )},
			new(0, "random",   "Insert Random Sample data")               { Callback = new ActionOrWriteException(subMenus.AddLoremIpsum), Hidden = true },
			new(0, "add",      "Add an asset")                            { Callback = new ActionOrWriteException(subMenus.AddAsset )},
			new(0, "filter",   "Manage active query")                     { Callback = new ActionOrWriteException(subMenus.FilterAssets )},
			new(0, "assets",   "Manage assets")                           { Callback = new ActionOrWriteException(subMenus.ManageAssets )},
			new(0, "office",   "Manage offices")                          { Callback = new ActionOrWriteException(subMenus.ManageOffices )},
			new(0, "currency", "Manage currencies")                       { Callback = new ActionOrWriteException(subMenus.ManageCurrencies )},
			new(),
			new(0, "list",     "List queried assets")                     { Callback = new ActionOrWriteException(subMenus.ListAssets )},
			new(0, "listo",    "List queried assets with office column")  { Callback = new ActionOrWriteException(subMenus.ListAssetsWithOffice )},
			new(0, "stat",     "Show asset statistics")                   { Callback = new ActionOrWriteException(subMenus.ShowStatistics )},
			new(0, "info",     "Show database connection information")    { Callback = new ActionOrWriteException(subMenus.Info )},
			new(),
			new(0, "nuke",     "Delete all assets")                       { Callback = new ActionOrWriteException(subMenus.NukeAssets )},
			new(0, "drop",     "Delete the database itself, then exit")   { Callback = DeleteDatabase },
			new(0, "exit",     "Exit the application")                    { Callback = Exit },
			new(0, "restart",  "Restart the application")                 { Callback = restart },
		];

		while (running)
		{
			mainMenu.Prompt("Main menu");
			Console.WriteLine();
		}
		return restartFlag ? Restart.yes : Restart.no;
	}
}