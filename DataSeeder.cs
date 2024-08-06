using Asset_Inventory.ExtensionMethods;
using AssetInventory.Data;
using AssetInventory.Logic;
using AssetInventory.Models;
using AssetInventory.Printing;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace AssetInventory;

internal static class DataSeeder
{
	private static readonly Random r = new();

	private static readonly string[] countryNames =
	[
		"Namibia",
		"Sudan",
		"Chad",
		"Eritrea",
		"Ivory Coast",
		"Eswatini",
		"Togo"
	];
	private static readonly string[] officeNames =
	[
		"Apple office",
				"Asset Headquarters",
				"Beans office",
				"Cold storage",
				"Contoso's team",
				"Crows office",
				"Greys office",
				"Karma office",
				"Legacy shop",
				"Pirate Rendezvous",
				"Research & Development",
				"River office",
				"Snake office",
				"The Coffee Break Corner",
				"The Conference Room Citadel",
				"The Cubicle Castle",
				"The Deadline Den",
				"The Desk Dungeon",
				"The Meeting Room Mansion",
				"The Office Oasis",
				"The Paper Pusher Plaza",
				"The Spreadsheet Sanctuary",
				"The Workaholic Tower",
				"Women's spa",
			];

	private static readonly string[] currencies =
	[
		"SEK",
		"DKK",
		"GBP",
		"USD",
		"CAD",
		"AUD",
	];

	private static T ChooseRandom<T>(IEnumerable<T> list)
	{
		return list.ElementAt(r.Next(list.Count()));
	}

	private static IEnumerable<string> GenerateAffixedNumber(string s)
	{
		yield return s;
		for (int i = 2; i > 1; i++)
			yield return s + " " + i;
	}

	private static bool OfficeExists(AssetInventoryDbContext ctx, string s)
	{
		return ctx.Offices.AsNoTracking().Where(o => o.Name == s).Any();
	}








	public static void AddOfficesAndSave(AssetInventoryDbContext ctx, int count)
	{
		while (count-- > 0)
		{
			string name = GenerateAffixedNumber(ChooseRandom(officeNames))
						  .First(s => !OfficeExists(ctx, s));
			ctx.Offices.Add(new()
			{
				Name = name,
				Country = ChooseRandom(countryNames),
			});
		}

		ctx.SaveChangesWithStatusAndLog();
	}

	public static void AddAssetsAndSave(AssetInventoryDbContext ctx, int count)
	{
		var types = AssetTypes;

		SpectreHelper.Loading().Start($"{count} items remaining", ctxStatus =>
		{
			while (0 < count--)
			{
				if (count % 100 == 0)
					ctxStatus.Status($"{count} items remaining");
				var x = new Asset()
				{
					Type = ChooseRandom(types),
					PriceCurrency = ChooseRandom(currencies),
					OfficeId = ChooseRandom(ctx.Offices.AsNoTracking()).OfficeId,
					Price = decimal.Round((decimal)(20d * r.NextDouble()), 2),
					ModelName = $"Lorem {r.Next(1_000)}00",
					PurchaseDate = DateTime.Now.AddDays(-r.Next(365 * 4)),
				};
				ctx.Assets.Add(x);
			}
		});

		ctx.SaveChangesWithStatusAndLog();
	}

	// use method instead?
	private static IEnumerable<AssetType> AssetTypes
	{
		get => new EnumEnumerable<AssetType>()
				.Where(t => t != AssetType.Unknown);
	}

}