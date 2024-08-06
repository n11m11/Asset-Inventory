using Asset_Inventory.Menus;
using System.Data.Common;
using AssetInventory.Models;
using AssetInventory.Printing;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;
using System.Text.Json;
using Asset_Inventory.JsonConfig;
using Asset_Inventory.ExtensionMethods;

namespace AssetInventory.Menus
{
    internal partial class _MenusClass
	{
		public static void ManageJson(JsonRepository jsonRepository)
		{
			try
			{
				jsonRepository.TryReading();
			}
			catch (JsonException ex)
			{
				AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything | ExceptionFormats.NoStackTrace);
				Console.WriteLine();
				Console.WriteLine("There were errors in the config file.");
				Console.WriteLine();
				ConsolePrompt.Pause();
				return;
			}
			catch (Exception ex)
			{
				if (ex is FileNotFoundException || ex is EmptyConfigException)
				{
					if (!AnsiConsole.Confirm("Would you like to create a new configuration file?"))
						return;
					try
					{
						jsonRepository.Create();
						Console.WriteLine();
					}
					catch (Exception ex2)
					{
						AnsiConsole.WriteException(ex2, ExceptionFormats.ShortenEverything | ExceptionFormats.NoStackTrace);
						Console.WriteLine();
						Console.WriteLine("Failed to create appsettings.json");
						Console.WriteLine();
						ConsolePrompt.Pause();
						return;
					}
				}
				else
				{
					throw;
				}
			}

			string? Select()
			{
				var keys = jsonRepository.GetAllConnectionStrings();
				if (keys.Count() == 0)
				{
					Console.WriteLine("There are no keys to select from.");
					ConsolePrompt.Pause();
					return null;
				}
				var prompt = new SelectionPrompt<KeyValuePair<string, string>>()
					.UseConverter(kvp =>
					{
						return string.IsNullOrEmpty(kvp.Value) ? kvp.Key : kvp.Key + " " + kvp.Value.RightEllipsis(80);
					});
				prompt.SearchEnabled = true; // address bug in Spectre.
				foreach (KeyValuePair<string, string> key in keys)
					prompt.AddChoice(key);
				return AnsiConsole.Prompt(prompt).Key;
			}
			string SelectType()
			{
				var keys = AutoConnectSchema.Types;
				var prompt = new SelectionPrompt<string>()
						.UseConverter(s => "type: " + s);
				prompt.SearchEnabled = true; // address bug in Spectre.
				foreach (string key in keys)
					prompt.AddChoice(key);
				return AnsiConsole.Prompt(prompt);
			}

			OrderedChoiceList choices =
			[
				new(-1,"add","Add a new connection string",new ActionOrWriteException(()=>
				{
					Console.WriteLine("Enter name for new connection string:");
					string key = ConsolePrompt.String(" > ");
					DbConnectionStringBuilder? dbcsb = ConnectionStringEditor.MSSQLLocalDB_Template;
					dbcsb = ConnectionStringEditor.EditInteractive(dbcsb);
					if(dbcsb != null)
						jsonRepository.SetConnectionString(key, dbcsb.ConnectionString);
				})),
				new(-1,"edit","Edit a connection string",new ActionOrWriteException(()=>
				{
					if(Select() is string key)
					{
						DbConnectionStringBuilder? dbcsb = new(){
							ConnectionString = jsonRepository.GetConnectionString(key)
						};
						dbcsb = ConnectionStringEditor.EditInteractive(dbcsb);
						if(dbcsb != null)
							jsonRepository.SetConnectionString(key, dbcsb.ConnectionString);
					}
				})),
				new(-1,"duplicate","Clone a connection string",new ActionOrWriteException(()=>
				{
					if(Select() is string key)
					{
						string target = ConsolePrompt.String("New key: ");
						if(string.IsNullOrEmpty(target))
						{
							Console.WriteLine("Cancel.");
							Console.WriteLine();
							return;
						}
						if(jsonRepository.GetConnectionString(key) is string cs)
							jsonRepository.SetConnectionString(target, cs);
					}
				})),
				new(-1,"delete","Remove a connection string",new ActionOrWriteException(()=>
				{
					if(Select() is string key)
						jsonRepository.RemoveConnectionString(key);
				})),
				new(),
				new(-1,"auto","Set auto connect connection string",new ActionOrWriteException(()=>
				{
					Console.WriteLine("Current key: " + jsonRepository.GetAutoConnect().Key);
					if(Select() is string key)
						if(SelectType() is string type)
							jsonRepository.SetAutoConnect(key,type);
				})),


				new(0,"","Return") { Hidden = true },
			];
			choices.AllowEmpty = true;
			do
			{
				if (choices.Prompt("Edit connection strings").Id == 0)
					break;
			} while (true);
		}
	}
}