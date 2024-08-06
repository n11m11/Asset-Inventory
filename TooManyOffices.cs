using AssetInventory.Logic;
using AssetInventory.Models;
using AssetInventory.Printing;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Spectre.Console;

namespace AssetInventory
{
	internal class TooManyOffices
	{
		internal static List<Asset> SelectAssetsByCountry(IIncludableQueryable<Asset, Office> assets)
		{
			if (!assets.Any())
			{
				Console.WriteLine("There are no assets to select from.");
				return new();
			}

			// we must do client-side evaluation here.
			IEnumerable<IGrouping<string, Asset>> assetByCountry = assets
				.AsEnumerable()
				.GroupBy(o => o.Office.Country);

			var multiPrompt = new MultiSelectionPrompt<Asset>()
			{
				Converter = StringFromAsset
			};
			foreach (var x in assetByCountry)
				if (x.Count() == 1)
					multiPrompt.AddChoice(x.First());
				else
					multiPrompt.AddChoiceGroup(new() { ModelName = x.Key }, x);
			return AnsiConsole.Prompt(multiPrompt);
		}

		internal static AssetType SelectAssetType()
		{
			OrderedChoiceList chooseType =
			[
				new((int)AssetType.Unknown,"",AssetType.Unknown.ToString()) { Hidden = true }, // default
					];
			chooseType.AllowEmpty = true;
			chooseType.AddRange(new EnumEnumerable<AssetType>().Select(type =>
				new OrderedChoice((int)type, type.ToString().ToLowerInvariant(), type + "")
				));
			return (AssetType)chooseType.Prompt("Type: ").Id;
		}

		internal static Office SelectOffice(IEnumerable<Office> offices)
		{
			if (!offices.Any())
				throw new InvalidOperationException("There are no offices to select from.");
			return AnsiConsole.Prompt(
								new SelectionPrompt<Office>()
								{
									SearchEnabled = true,
									Converter = StringFromOffice,
								}.AddChoices(offices).Title("Pick an office: "));
		}

		internal static List<Office> SelectOffices(IEnumerable<Office> offices)
		{
			if (!offices.Any())
			{
				Console.WriteLine("There are no offices to select from.");
				return new();
			}
			// we must do client-side evaluation here.
			IEnumerable<IGrouping<string, Office>> officeByCountry = offices.GroupBy(o => o.Country);
			var multiPrompt = new MultiSelectionPrompt<Office>()
			{
				Converter = StringFromOffice
			};
			foreach (var x in officeByCountry)
				if (x.Count() == 1)
					multiPrompt.AddChoice(x.First());
				else
					multiPrompt.AddChoiceGroup(new() { Country = x.Key }, x);
			return AnsiConsole.Prompt(multiPrompt);

		}
		internal static string StringFromAsset(Asset a)
		{
			if (a.Office?.Country is string country)
				return $"{a.ModelName} ({a.Office.Name}, {country})";
			return a.ModelName;
		}

		internal static string StringFromOffice(Office o) =>
															o.Name.IsNullOrEmpty() ?
															o.Country :
															$"{o.Name} ({o.Country})";

		internal static Table TableFromAssets(IEnumerable<Asset> assets)
		{
			var t = new Table()
			{
				Title = new("Assets")
			}
			.AddColumn("Model")
			.AddColumn("Office")
			.AddColumn("Purchased");
			foreach (var x in assets)
				t.AddRow(
					x.ModelName,
					x.Office is null ? "" : StringFromOffice(x.Office),
					x.PurchaseDate + "");
			return t;
		}

		internal static Table TableFromOffices(IEnumerable<Office> o)
		{
			var t = new Table()
			{
				Title = new("Offices")
			}
			.AddColumn("ID")
			.AddColumn("Name")
			.AddColumn("Country");
			foreach (var x in o.OrderBy(o => o.Country))
				t.AddRow(
					x.OfficeId + "",
					x.Name,
					x.Country);
			return t;
		}

	}
}