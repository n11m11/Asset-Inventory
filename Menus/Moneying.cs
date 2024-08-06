using AssetInventory.HttpConnectivity;
using AssetInventory.Logic;
using AssetInventory.Models;
using AssetInventory.Printing;
using Spectre.Console;

namespace AssetInventory.Menus
{
	internal partial class _MenusClass
	{
		ExchangeRateApiRepository _apirepo = null!;
		internal string currencyApiKey { get; set; } = null!;
		internal ExchangeRateApiRepository CurrencyApiRepo
		{
			get
			{
				return _apirepo ??= new(ctx)
				{
					ApiKey = currencyApiKey,
				};
			}
		}

		public void ManageCurrencies()
		{
			bool running = true;


			OrderedChoiceList choices =
			[
				new(-1,"cache","Show cached JSON response for currency", ()=>
					{
						var currencyCodes = CurrencyApiRepo.GetCachedKeys();
						if(!currencyCodes.Any())
						{
							Console.WriteLine("There are no currencies in the cache.");
							ConsolePrompt.Pause();
							return;
						}
						var code = SelectCurrency(currencyCodes);
						if (CurrencyApiRepo.CacheFind(code) is HttpCache row)
							Console.WriteLine(row.Body);
						else
							Console.WriteLine("empty string");
					}),
				new (-1,"get","Query exchange rate API", ()=>
					{
						Currency code = SelectCurrency();
						try
						{
							HttpCache hc = CurrencyApiRepo.CacheCtx.GetUri(code);
							Console.WriteLine(hc.Body);
						}
						catch (Exception ex)
						{
							AnsiConsole.WriteException(ex,ExceptionFormats.ShortenEverything|ExceptionFormats.NoStackTrace);
						}
					}),
				new (-1,"rate","Get conversion rate", ()=>
					{
						var from = SelectCurrency(title:"From:");
						var to = SelectCurrency(title:"To:");
						try
						{
							Console.WriteLine($"{from.ToString()} -> {to.ToString()}:");
							Console.WriteLine((double)CurrencyApiRepo.GetRate(from,to));
							ConsolePrompt.Pause();
						}
						catch (Exception ex)
						{
							AnsiConsole.WriteException(ex,ExceptionFormats.ShortenEverything|ExceptionFormats.NoStackTrace);
						}
					}),
				new (-1,"clear","Clear JSON response cache", ()=>
					{
						var count = CurrencyApiRepo.GetCachedKeys()?.Count()??0;
						Console.WriteLine($"Clearing {count} cache entity{(count==1?"":"s")}.");
						CurrencyApiRepo.RemoveAll();
					}),
				new(0, "", "Return to main menu") { Hidden = true },
			];
			choices.AllowEmpty = true;
			Console.WriteLine();
			Console.WriteLine("The API used:");
			Console.WriteLine("  https://www.exchangerate-api.com/");
			Console.WriteLine();
			do
			{
				Console.WriteLine("Api stats for this session");
				Console.WriteLine($"  Api hits:       {CurrencyApiRepo.CacheCtx.api_hits}");
				Console.WriteLine($"  Cache hits:     {CurrencyApiRepo.CacheCtx.cache_hits}");
				Console.WriteLine($"  Cache misses:   {CurrencyApiRepo.CacheCtx.cache_misses}");
				Console.WriteLine($"  Cache expiries: {CurrencyApiRepo.CacheCtx.cache_expiries}");
				Console.WriteLine($"  API key: {!string.IsNullOrWhiteSpace(currencyApiKey)}");
				Console.WriteLine();
				var id = choices.Prompt("Main Menu » Manage currencies").Id;
				Console.WriteLine();
				if (id == 0)
					break;
			} while (running == true);
		}

		private static Currency SelectCurrency(IEnumerable<Currency> list = null, string title = "Enter a currency code")
		{
			var searchable_prompt = new SelectionPrompt<Currency>()
			{
				SearchPlaceholderText = "USD, GBP, RMB...",
				Title = title,
			}
				.AddChoices(
					list ?? new EnumEnumerable<Currency>()
				)
				.UseConverter(
					x => CurrencyInfo.CurrencyData[x.ToString()].CurrencyName
				)
				.EnableSearch();
			Currency x = AnsiConsole.Prompt<Currency>(searchable_prompt);
			Console.WriteLine("Selected: " + x);
			return x;
		}
	}
}