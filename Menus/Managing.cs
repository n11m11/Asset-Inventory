using System.Text.RegularExpressions;
using Asset_Inventory.ExtensionMethods;
using AssetInventory.Models;
using AssetInventory.Printing;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Spectre.Console;

namespace AssetInventory.Menus
{
    internal partial class _MenusClass
	{
		public void ManageOffices()
		{
			bool running = true;

			Action ListAllOffices = () =>
			{
				AnsiConsole.Write(new Padder(TooManyOffices.TableFromOffices(ctx.Offices)));
			};

			Action AddOffice = () =>
			{
				var o = new Office()
				{
					Name = ConsolePrompt.String("Office name: "),
					Country = ConsolePrompt.String("Country: "),
				};
				if (AnsiConsole.Confirm("Add this office?"))
				{
					ctx.Add(o);
					ctx.SaveChangesWithStatusAndLog();
				}
			};

			void EditOffices(bool editAll = false)
			{
				if (!ctx.Offices.Any())
				{
					AnsiConsole.Write(new Padder(new Text("There are no offices to edit.")));
					ConsolePrompt.Pause();
					return;
				}

				bool running = true;
				List<Office> selectedOffices;
				if (editAll)
					selectedOffices = ctx.Offices.Include(o => o.Assets).AsEnumerable().ToList();
				else
					selectedOffices = TooManyOffices.SelectOffices(ctx.Offices.Include(o => o.Assets).AsEnumerable());

				void ShowSelection() =>
					AnsiConsole.Write(new Padder(new Rows(new Text("You selected these offices:"), TooManyOffices.TableFromOffices(selectedOffices))));

				ShowSelection();

				void DeleteOffices()
				{
					Console.WriteLine("This will delete all associated assets.");
					var count = selectedOffices.Count;
					var assetCount = selectedOffices.Select(o => o.Assets.Count).Sum();
					if (!AnsiConsole.Confirm($"Delete {count} offices with {assetCount} assets?"))
						return;
					foreach (Office o in selectedOffices)
						ctx.Offices.Remove(o);
					ctx.SaveChangesWithStatusAndLog();
				}
				void SetCountry()
				{
					Console.WriteLine("Leave blank to cancel.");
					string s = ConsolePrompt.String("Country name: ");
					if (s.IsNullOrEmpty())
						return;
					foreach (Office o in selectedOffices)
						o.Country = s;
					ctx.SaveChangesWithStatusAndLog();
				}
				void RegexReplaceOfficeNames()
				{
					if (SpectreHelper.PromptRegex() is Regex pattern)
					{
						string replacement = ConsolePrompt.String("Regex replacement: ");
						foreach (Office o in selectedOffices)
							o.Name = pattern.Replace(o.Name, replacement);
						ctx.SaveChangesWithStatusAndLog();
					}
				}


				OrderedChoiceList choiceEditMenu =
				[
					new(-1,"show","Show selected offices")            { Callback = ShowSelection },
					new(-1,"country","Set country name")              { Callback = SetCountry },
					new(-1,"regex","Regex replace on office names")   { Callback = RegexReplaceOfficeNames },
					new( 0,"delete","Delete selected offices")        { Callback = DeleteOffices },
					new(),
					new( 0,"cancel","Return to office menu"),
					new( 0,"","Return to office menu") { Hidden = true },
				];
				choiceEditMenu.AllowEmpty = true;
				int id;
				do
				{
					id = choiceEditMenu.Prompt("Main Menu » Manage offices » Edit selected").Id;
					Console.WriteLine();
				} while (running && id != 0);
			}

			void Exit()
			{
				running = false;
			}


			OrderedChoiceList choicesOffice =
			[
				new(-1,"list","List all offices")                 { Callback = ListAllOffices  },
				new(-1,"add","Add an office")                     { Callback = AddOffice  },
				new(-1,"edit","Edit offices")                     { Callback = ()=>EditOffices()  },
				new(-1,"all","Edit all offices")                  { Callback = ()=>EditOffices(true)  },
				new(),
				new(-1,"exit","Return to main menu")              { Callback = Exit  },
				new(-1,"","Return to main menu")                  { Callback = Exit, Hidden = true  },
			];

			do
			{
				choicesOffice.Prompt("Main Menu » Manage offices");
				Console.WriteLine();
			} while (running);
		}

		public void ManageAssets()
		{
			if (!assetQueryChain.AggregateQuery.Any())
			{
				AnsiConsole.Write(new Padder(new Text("There are no assets in your query to manage.")));
				ConsolePrompt.Pause();
				return;
			}

			List<Asset> selection = null!;

			void ShowSelection()
			{
				AnsiConsole.Write(new Padder(new Rows(new Text("You selected these assets:"), TooManyOffices.TableFromAssets(selection))));
			}

			void SelectAll()
			{
				selection = assetQueryChain.AggregateQuery.AsEnumerable().ToList();
			}
			void SelectByCountry()
			{
				var assets = assetQueryChain.AggregateQuery.Include(a => a.Office);
				selection = TooManyOffices.SelectAssetsByCountry(assets);
				ShowSelection();
			}
			OrderedChoiceList list = [
				new(1,"all","Mass edit all queried assets")                    { Callback = SelectAll },
				new(1,"select","Mass edit a selection of the queried assets")  { Callback = SelectByCountry },
				new(0,"","Return to main menu")                                { Hidden = true },
			];
			list.AllowEmpty = true;
			int choiceId = list.Prompt($"You have queried {assetQueryChain.AggregateQuery.Count()} assets.").Id;
			if (choiceId != 0)
			{
				Console.WriteLine($"You have selected {selection.Count} assets.");

				bool running = true;

				void DeleteSelected()
				{
					running = false;
					foreach (Asset a in selection)
						ctx.Assets.Remove(a);
					ctx.SaveChangesWithStatusAndLog();
				}
				void SetPrice()
				{
					Decimal d = ConsolePrompt.Decimal("New price: ", validate: ConsolePrompt.IsPositiveCurrency);
					foreach (Asset a in selection)
						a.Price = d;
					ctx.SaveChangesWithStatusAndLog();
				}
				void SetOffice()
				{
					Office o = TooManyOffices.SelectOffice(ctx.Offices.AsNoTracking());
					foreach (Asset a in selection)
						a.OfficeId = o.OfficeId;
					ctx.SaveChangesWithStatusAndLog();
				}
				void RegexReplaceModelNames()
				{
					if (SpectreHelper.PromptRegex() is Regex pattern)
					{
						string replacement = ConsolePrompt.String("Replacement: ");
						foreach (Asset a in selection)
							a.ModelName = pattern.Replace(a.ModelName, replacement);
						ctx.SaveChangesWithStatusAndLog();
					}
				}
				void Exit()
				{
					running = false;
				}
				OrderedChoiceList choiceEditMenu =
				[
					new(-1,"show","Show selected assets")                { Callback = ShowSelection },
					new(-1,"price","Set price")                          { Callback = SetPrice },
					new(-1,"office","Set office")                        { Callback = SetOffice },
					new(-1,"regex","Regex replace on asset model names") { Callback = RegexReplaceModelNames },
					new(-1,"delete","Delete selected assets")            { Callback = DeleteSelected },
					new(-1,"","Return to office menu")                   { Callback = Exit, Hidden = true },
				];
				choiceEditMenu.AllowEmpty = true;
				do
				{
					choiceEditMenu.Prompt("Main Menu » Manage assets");
					Console.WriteLine();
				} while (running == true);
			}
		}
	}
}