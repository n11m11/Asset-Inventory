using AssetInventory.Logic;
using AssetInventory.Models;
using AssetInventory.Printing;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace AssetInventory.Menus
{
	internal partial class _MenusClass
	{
		public void ListAssets()
		{
			SpectreHelper.SpectreTryCatchAll<Exception>(resetConsoleOnError: true, action: () =>
			{
				var t = new Table()
				{
					Border = TableBorder.Rounded,
					Expand = true,
					ShowFooters = true,
				}
				.AddColumn(new TableColumn("Type") { Footer = new Text("Type"), NoWrap = true, Alignment = Justify.Center, })
				.AddColumn(new TableColumn("Model") { Footer = new Text("Model"), NoWrap = false, Alignment = Justify.Left, })
				.AddColumn(new TableColumn("Price") { Footer = new Text("Price"), NoWrap = true, Alignment = Justify.Right, })
				.AddColumn(new TableColumn("Purchased") { Footer = new Text("Purchased"), NoWrap = false, Alignment = Justify.Left, });

				var query = assetQueryChain.AggregateQuery.AsNoTracking()
														  .OrderByDescending(a => (int)a.Type)
														  .ThenBy(a => a.PurchaseDate);

				DateTime minPurchaseDate = DateTime.Now.AddYears(-3).AddMonths(3);

				// New Memoizer class.
				// Is this functional code readable?
				var styleForDate = new Memoizer<bool, Style>(isRed =>
					Style.Parse(isRed ? "red" : "green")
					);
				var renderableFromAssetType = new Memoizer<AssetType, Text>(a =>
					new Text(a + "", ColorHash.QuickHash(a + ""))
					);

				foreach (Asset a in query)
				{
					bool isRed = a.PurchaseDate is not null && a.PurchaseDate < minPurchaseDate;
					string price = a.Price is null ? "" : string.IsNullOrEmpty(a.PriceCurrency) ? $"{a.Price}" : $"{a.Price} {a.PriceCurrency}";
					string sDate = a.PurchaseDate is null ? "" : a.PurchaseDate.Value.ToString("yyyy-MM-dd HH:mm:ss");


					t.AddRow(
							 renderableFromAssetType[a.Type],
							 new Markup(Markup.Escape(a.ModelName)).Overflow(Overflow.Ellipsis),
							 new Markup(a.Price + ""),
							 new Text(sDate, style: styleForDate[isRed])
							 );
				}
				AnsiConsole.Write(t);
			});
			ConsolePrompt.Pause();
		}
		public void ListAssetsWithOffice()
		{
			SpectreHelper.SpectreTryCatchAll<Exception>(resetConsoleOnError: true, action: () =>
			{
				var t = new Table()
				{
					Border = TableBorder.Rounded,
					Expand = true,
					ShowFooters = true,
				}
				.AddColumn(new TableColumn("Office") { Footer = new Text("Office"), NoWrap = true, Alignment = Justify.Center, Padding = null })
				.AddColumn(new TableColumn("Type") { Footer = new Text("Type"), NoWrap = true, Alignment = Justify.Center, Padding = null })
				.AddColumn(new TableColumn("Model") { Footer = new Text("Model"), NoWrap = true, Alignment = Justify.Left, Padding = null })
				.AddColumn(new TableColumn("Price") { Footer = new Text("Price"), NoWrap = true, Alignment = Justify.Right, Padding = null })
				.AddColumn(new TableColumn("Purchased") { Footer = new Text("Purchased"), NoWrap = true, Alignment = Justify.Left, Padding = null });

				var query = assetQueryChain.AggregateQuery.AsNoTracking()
														  .OrderBy(a => a.Office)
														  .ThenBy(a => a.PurchaseDate);

				DateTime minPurchaseDate = DateTime.Now.AddYears(-3).AddMonths(3);
				DateTime minPurchaseDate2 = DateTime.Now.AddYears(-3).AddMonths(6);

				// New Memoizer class.
				// Is this functional code readable?
				var styleForDate = new Memoizer<DateTime, Style>(date =>
						Style.Parse(
							date switch
							{
								_ when date < minPurchaseDate => "red",
								_ when date < minPurchaseDate2 => "yellow",
								_ => "green",
							}
						)
					);
				var renderableFromAssetType = new Memoizer<AssetType, Text>(at =>
					new Text(at + "", ColorHash.QuickHash(at + ""))
					);
				var renderableFromOffice = new Memoizer<Office?, int, Text>(
					o =>
						o == null ?
						new Text("") :
						new Text(TooManyOffices.StringFromOffice(o), ColorHash.QuickHash(TooManyOffices.StringFromOffice(o)))
,
					o =>
						o == null ? -1 : o.OfficeId);

				foreach (Asset a in query.Include(x => x.Office))
				{
					Style? style = a.PurchaseDate is null ? null : styleForDate[a.PurchaseDate.Value];
					string sDate = a.PurchaseDate is null ? "" : a.PurchaseDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
					t.AddRow(
							 renderableFromOffice[a.Office],
							 renderableFromAssetType[a.Type],
							 new Markup(Markup.Escape(a.ModelName)).Overflow(Overflow.Ellipsis),
							 new Markup(a.Price + ""),
							 new Text(sDate, style: style)
							 );
				}

				AnsiConsole.Write(t);
			});
			ConsolePrompt.Pause();
		}
	}
}