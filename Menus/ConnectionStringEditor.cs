using System.Data.Common;
using AssetInventory.Printing;
using Spectre.Console;

namespace Asset_Inventory.Menus;

internal static class ConnectionStringEditor
{
	public static DbConnectionStringBuilder? EditInteractive(DbConnectionStringBuilder connectionStringBuilder)
	{
		string? SelectBuilderKey(string ErrorWhenEmpty)
		{
			var keys = connectionStringBuilder.Keys;
			if (keys.Count == 0)
			{
				if (!string.IsNullOrEmpty(ErrorWhenEmpty))
				{
					Console.WriteLine(ErrorWhenEmpty);
					Console.WriteLine();
					ConsolePrompt.Pause();
				}
				return null;
			}
			var sel = new SelectionPrompt<string>().EnableSearch();
			foreach (string x in keys)
				sel.AddChoice(x);
			return AnsiConsole.Prompt(sel);
		}

		OrderedChoiceList choices = [
			new(-1,"add","Add a key to the connection string", ()=>
			{
				if(ConsolePrompt.String("Enter key name: ").Trim() is string key)
					connectionStringBuilder.Add(key, ConsolePrompt.String("Enter value: "));
				else
					Console.WriteLine("Skipped adding empty key.");
			}),
			new(-1,"edit","Edit a key", ()=>
			{
				string errorMsg = "There are no keys to edit.";
				if(SelectBuilderKey(ErrorWhenEmpty: errorMsg) is string key)
				{
					var rl = new SimplerGnuReadLine(key+": ");
					connectionStringBuilder[key] = rl.ReadLine((string)connectionStringBuilder[key],true,true);
				}
			}),
			new(-1,"delete","Delete a key", ()=>
			{
				string errorMsg = "There are no keys to delete.";
				if(SelectBuilderKey(ErrorWhenEmpty: errorMsg) is string key)
				{
					connectionStringBuilder.Remove(key);
				}
			}),
			new(),
			new(-1,"set","Set connection string directly", ()=>
			{
				Console.WriteLine("Enter connection string:");
				try
				{
					connectionStringBuilder.ConnectionString = Console.ReadLine();
				}
				catch (Exception ex)
				{
					AnsiConsole.WriteException(ex,ExceptionFormats.ShortenEverything|ExceptionFormats.NoStackTrace);
					Console.WriteLine();
					Console.WriteLine("There was a problem in the connection string.");
					Console.WriteLine();
					ConsolePrompt.Pause();
				}
			}),
			new(-1,"reset","Clear all keys", ()=>
			{
				connectionStringBuilder.Clear();
			}),
			new(-1,"localdb","Reset using MSSQLLocalDB template", ()=>
			{
				connectionStringBuilder = MSSQLLocalDB_Template;
			}),
			new(-1,"sqlite","Reset using SQLite template", ()=>
			{
				connectionStringBuilder = Sqlite_Template;
			}),
			new(),
			new(-1,"show","Print connection string", ()=>
			{
				Console.WriteLine(connectionStringBuilder.ConnectionString);
				Console.WriteLine();
				ConsolePrompt.Pause();
			}),
			new(0,"cancel","Return without creating a connection string",()=>
			{
				connectionStringBuilder = null!;
			}),
			new(0,"done","Finish editing"),
			new(0,"","Press enter to return") { Hidden = true },
		];
		choices.AllowEmpty = true;

		do
		{
			Console.WriteLine();
			Console.WriteLine("Keys:");
			var keys = connectionStringBuilder.Keys;
			if (keys.Count != 0)
			{
				var t = new Table()
					.AddColumn(new("Key"))
					.AddColumn(new("Value"));
				foreach (string x in keys)
					t.AddRow(x, (string)connectionStringBuilder[x]);
				AnsiConsole.Write(t);
			}
			else
			{
				Console.WriteLine("  No keys.");
				Console.WriteLine();
			}


			int id = choices.Prompt().Id;
			if (id == 0)
				break;

		} while (true);
		return connectionStringBuilder;
	}


	public static DbConnectionStringBuilder MSSQLLocalDB_Template
	{
		get => new DbConnectionStringBuilder()
		{
			{ "Data Source", @"(localdb)\MSSQLLocalDB" },
			{ "Initial Catalog", "AssetInventory" },
			{ "Integrated Security", true },
			{ "Connect Timeout", 30 },
			{ "Encrypt", false },
			{ "Trust Server Certificate", false },
			{ "Application Intent", "ReadWrite" },
			{ "Multi Subnet Failover", false },
		};
	}
	public static DbConnectionStringBuilder Sqlite_Template
	{
		get => new DbConnectionStringBuilder()
		{
			{ "Data Source", @"AssetInventory.db" },
		};
	}
}
