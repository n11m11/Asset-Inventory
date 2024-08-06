using AssetInventory.Printing;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace AssetInventory.Menus
{
	internal partial class _MenusClass
	{
		public void AddLoremIpsum()
		{
			if (!ctx.Offices.AsNoTracking().Any())
			{
				AnsiConsole.WriteLine("Create an office before generating assets.");
				return;
			}

			Console.WriteLine();
			Console.WriteLine("How many should I insert?");
			int ResponseHowMany = (int)ConsolePrompt.Decimal(" > ", "1", validate: x => x % 1 == 0 && 0 <= x && x <= 1_000_000);
			if (ResponseHowMany > 1000)
			{
				Console.WriteLine("You've entered a large number. Are you sure you want to continue?");
				if (!AnsiConsole.Confirm("Continue?"))
					return;
			}

			DataSeeder.AddAssetsAndSave(ctx, ResponseHowMany);
		}

		public void Seed()
		{
			DataSeeder.AddOfficesAndSave(ctx, 50);
			DataSeeder.AddAssetsAndSave(ctx, 100);
		}

		public void SeedOffices()
		{
			DataSeeder.AddOfficesAndSave(ctx, 50);
		}
	}
}