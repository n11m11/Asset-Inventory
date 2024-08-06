using Asset_Inventory.ExtensionMethods;
using AssetInventory.Models;
using AssetInventory.Printing;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace AssetInventory.Menus
{
    internal partial class _MenusClass
	{
		public void AddAsset()
		{
			if (!ctx.Offices.AsNoTracking().Any())
			{
				AnsiConsole.WriteLine("Create an office before adding assets.");
				ConsolePrompt.Pause();
				return;
			}
			var type = TooManyOffices.SelectAssetType();
			var office = TooManyOffices.SelectOffice(ctx.Offices);
			Console.WriteLine(office.ToString());
			var modelName = ConsolePrompt.String("ModelName: ");
			var price = ConsolePrompt.Decimal("Price: ", validate: ConsolePrompt.IsPositiveCurrency);
			var currency = ConsolePrompt.String("Currency: ");
			AnsiConsole.WriteLine("Purchase date:");
			var date = ConsolePrompt.DatePicker().ToDateTime(new());
			Asset a = new()
			{
				Type = type,
				Office = office,
				ModelName = modelName,
				Price = price,
				PriceCurrency = currency,
				PurchaseDate = date,
			};
			ctx.Assets.Add(a);
			ctx.SaveChangesWithStatusAndLog();
		}

		public void FilterAssets()
		{
			Console.Write(AnsiTools.AlternateScreenBufferOn + "\u001bc\x1b[1;1H");
			bool running = true;
			Action AddContains = () =>
			{
				string s = ConsolePrompt.String("Model name shall contain: ");
				assetQueryChain.Add(q => q.Where(x => x.ModelName.Contains(s)));
			};
			Action AddAfter = () =>
			{
				DateTime date = ConsolePrompt.DatePicker().ToDateTime(new TimeOnly());
				assetQueryChain.Add(q => q.Where(x => x.PurchaseDate == null || x.PurchaseDate > date));
			};
			Action AddBefore = () =>
			{
				DateTime date = ConsolePrompt.DatePicker().ToDateTime(new TimeOnly());
				assetQueryChain.Add(q => q.Where(x => x.PurchaseDate == null || x.PurchaseDate < date));
			};
			Action Reset = () =>
			{
				assetQueryChain.Reset();
			};
			Action Exit = () =>
			{
				running = false;
			};
			Action _ListAssets = () =>
			{
				ListAssets();
				ConsolePrompt.Pause();
			};
			Action _ListAssetsWithOffice = () =>
			{
				ListAssetsWithOffice();
				ConsolePrompt.Pause();
			};
			OrderedChoiceList choiceList =
			[
				new(0,"list","List queried assets")                       { Callback = _ListAssets },
				new(0,"listo","List queried assets with office column")   { Callback = _ListAssetsWithOffice },
				new(0,"contains","Model name shall contain...")           { Callback = AddContains },
				new(0,"after","Purchase date shall be after...")          { Callback = AddAfter },
				new(0,"before","Purchase date shall be before...")        { Callback = AddBefore },
				new(0,"reset","Reset the filters")                        { Callback = Reset },
				new(0,"","Press enter to return")                         { Callback = Exit, Hidden=true },
			];
			choiceList.AllowEmpty = true;
			while (running)
			{
				Console.Clear();
				choiceList.Prompt($"Main Menu » Add Filters\n\n{assetQueryChain.FilterCount} search filters added.\n{assetQueryChain.AggregateQuery.Count()} assets in query.\n");
			}
			Console.Write("\u001bc\x1b[1;1H" + AnsiTools.AlternateScreenBufferOff);
		}
	}
}