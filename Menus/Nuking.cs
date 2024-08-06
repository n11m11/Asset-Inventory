using Asset_Inventory.ExtensionMethods;
using AssetInventory.Models;
using AssetInventory.Printing;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace AssetInventory.Menus
{
    internal partial class _MenusClass
	{
		public void NukeAssets()
		{
			if (ConsolePrompt.String("Are you sure? Type `YES' to continue.\n >> ") != "YES")
				return;
			Console.WriteLine(@"          _ ._  _ , _ ._");
			Console.WriteLine(@"        (_ ' ( `  )_  .__)");
			Console.WriteLine(@"      ( (  (    )   `)  ) _)");
			Console.WriteLine(@"     (__ (_   (_ . _) _) ,__)");
			Console.WriteLine(@"         `~~`\ ' . /`~~`");
			Console.WriteLine(@"              ;   ;");
			Console.WriteLine(@"              /   \");
			Console.WriteLine(@"_____________/_ __ \_____________");
			int count = SpectreHelper.Loading()
				.Start($"Connecting to database", _ =>
					ctx.Assets.Count()
				);
			SpectreHelper.Loading()
				.Start($"Connecting", ctxStatus =>
				{
					if (ctx.Database.IsInMemory())
					{
						ctxStatus.Status("Deleting items individually");
						foreach (Asset x in ctx.Assets)
							ctx.Remove(x);
					}
					else
					{
						ctxStatus.Status("Deleting items");
						ctx.Assets.ExecuteDelete();
					}
				}
				);
			ctx.SaveChangesWithStatusAndLog();
			Console.WriteLine($"{count - ctx.Assets.Count()} rows deleted.");
		}

		public bool _bool_try_to_DeleteDatabase()
		{
			if (ConsolePrompt.String("This operation has not been well tested. Type `YES' to proceed.\n >> ") != "YES")
				return false;
			try
			{
				if (ctx.Database.TestConnection(out Exception? ex))
				{
					if (ctx.Database.EnsureDeleted())
						Console.WriteLine("The database has been deleted.");
					else
						Console.WriteLine("The database doesn't exist.");
					Console.WriteLine();
					Console.WriteLine("Press enter to continue.");
					Console.ReadLine();
					return true;
				}
				throw ex;
			}
			catch (Exception ex)
			{
				AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything | ExceptionFormats.NoStackTrace);
				return false;
			}
		}
	}
}