using System.Data.Common;
using System.Diagnostics;
using System.Security;
using Asset_Inventory.ExtensionMethods;
using Asset_Inventory.JsonConfig;
using Asset_Inventory.Menus;
using AssetInventory.Data;
using AssetInventory.Printing;
using Microsoft.IdentityModel.Tokens;
using Spectre.Console;

namespace AssetInventory.Menus;

internal class _InteractiveSetup
{


	internal static AssetInventoryDbContext? InteractiveSetupMenu()
	{
		AssetInventoryDbContext? ctx = null;

		bool quitApplicationFlag = false;
		bool continueFlag = false;


		void CreateOrDispose(bool migrate)
		{
			Exception? ex = null;
			SpectreHelper.Loading().Start("Creating database", _ =>
			{
				if (!ctx.Database.TryCreate(migrate, out ex))
				{
					ctx.Dispose();
					ctx = null;
				}
			});
			if (ex is not null)
				AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything | ExceptionFormats.NoStackTrace);
		}

		void ConnectOrDispose()
		{
			if (!ctx.Database.CanConnect())
			{
				Console.WriteLine("CanConnect() returned false. Ensure that the database has been created first.");
				ctx.Dispose();
				ctx = null;
				return;
			}
			if (!ctx.Database.TestConnection(out Exception? ex))
			{
				if (ex is not null)
					AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything | ExceptionFormats.NoStackTrace);
				Console.WriteLine("TestConnection() returned false. Ensure that the database has been created first.");
				ctx.Dispose();
				ctx = null;
			}
		}

		bool AskMigrate() => AnsiConsole.Confirm("Use migrations? (recommended)");

		Action CreateSqlite = () =>
		{

			string? path = SQLitePicker(pickNewFile: true);

			if (path is null)
				continueFlag = true;


			if (path is not null)
			{
				bool migrate = AskMigrate();

				if (!AnsiConsole.Confirm("Create database?"))
					return;

				if (path == ":memory:")
				{
					ctx = new AssetInventoryDbContextSqlite() { UseInMemory = true };
					CreateOrDispose(migrate: migrate);
				}
				else
				{
					var dbcsb = new DbConnectionStringBuilder() { { "Data Source", path } };
					ctx = new AssetInventoryDbContextSqlite() { ConnectionString_Sqlite = dbcsb.ConnectionString };
					CreateOrDispose(migrate: migrate);
				}
			}
		};

		Action ConnectSqlite = () =>
		{
			Console.WriteLine("Sqlite in-memory is ");
			Console.WriteLine();
			string? path = SQLitePicker(pickNewFile: false);

			if (path is null)
			{
				continueFlag = true;
				return;
			}

			if (path is not null)
			{
				var dbcsb = new DbConnectionStringBuilder() { { "Data Source", path } };
				ctx = new AssetInventoryDbContextSqlite() { ConnectionString_Sqlite = dbcsb.ConnectionString };
				ConnectOrDispose();
			}
		};

		Action CreateMssql = () =>
		{
			Console.WriteLine("Are you sure that you wish to create a database in LocalDB instead of using Sqlite?");
			if (ConsolePrompt.String("Proceed only if you know what you are doing. Type `YES' to continue\n >> ") != "YES")
			{
				continueFlag = true;
				return;
			}

			bool migrate = AskMigrate();

			if (!AnsiConsole.Confirm("Create database?"))
				return;

			ctx = new AssetInventoryDbContextSqlServer()
			{
				ConnectionString_SqlServer = ConnectionStringEditor.MSSQLLocalDB_Template.ConnectionString
			};
			CreateOrDispose(migrate: migrate);
		};

		Action ConnectMssql = () =>
		{
			ctx = new AssetInventoryDbContextSqlServer()
			{
				ConnectionString_SqlServer = ConnectionStringEditor.MSSQLLocalDB_Template.ConnectionString
			};
			ConnectOrDispose();
		};

		Action EditConnectionString = () =>
		{
			JsonRepository jsonRepository = new();
			_MenusClass.ManageJson(jsonRepository);
		};

		Action ConnectInMemory = () =>
		{
			ctx = new AssetInventoryDbContextInMemory();
			ConnectOrDispose();
		};

		Action Exit = () =>
		{
			quitApplicationFlag = true;
		};

		do
		{
			continueFlag = false;

			OrderedChoiceList choices =
			[
				new(-1,"memory","Create a disposable in-memory database")                            { Callback = ConnectInMemory },
				new(),
				new(-1,"create","Create an SQLite database file (recommended)")                      { Callback = CreateSqlite },
				new(-1,"sqlite","Open a previously created SQLite file")                             { Callback = ConnectSqlite },
				new(),
				new(-1,"localdb","Connect to database `AssetInventory' in (localdb)\\MSSQLLocalDB")  { Callback = ConnectMssql },
				new(-1,"createmssql","Create database `AssetInventory' in (localdb)\\MSSQLLocalDB")  { Callback = CreateMssql },
				new(-1,"exit","Exit the application")                                                { Callback = Exit, Hidden = true },
				new(),
				new(-1,"edit","Edit appsettings.json")                                               { Callback = EditConnectionString },
				new(-1,"json","Edit appsettings.json in notepad",()=>
				{
					try
					{
						new Process{
							StartInfo = new(new JsonRepository().FilePath)
							{
								UseShellExecute = true,
							}
						}.Start();
					}
					catch (Exception ex)
					{
						AnsiConsole.WriteException(ex,ExceptionFormats.ShortenEverything|ExceptionFormats.NoStackTrace);
						Console.WriteLine();
						ConsolePrompt.Pause();
					}
				}),
				new(-1,"folder","Show appsettings.json in file explorer",()=>
				{
					try{
						if(Path.GetDirectoryName(new JsonRepository().FilePath) is string directory)
						new Process{
							StartInfo = new(directory)
							{
								UseShellExecute = true,
							}
						}.Start();
					}
					catch (Exception ex)
					{
						AnsiConsole.WriteException(ex,ExceptionFormats.ShortenEverything|ExceptionFormats.NoStackTrace);
						Console.WriteLine();
						ConsolePrompt.Pause();
					}
				}),
			];
			IEnumerable<OrderedChoice> pathSuggestions = SqlitePathSuggestions().Where(x => File.Exists((string)x.Data!));
			if (pathSuggestions.Any())
			{
				choices.Add(new());
				choices.Add(new() { Longname = "[white]  SQLite database files:[/]" });
			}
			foreach (OrderedChoice x in pathSuggestions)
				choices.Add(x with
				{
					Id = -1,
					Longname = $"Existing SQLite ({((string)x.Data!).LeftEllipsis(39)})"
				});

			OrderedChoice choice = choices.Prompt("Which database would you like to connect to?");
			Console.WriteLine();

			if (quitApplicationFlag)
				break;

			if (choice.Data is string path)
			{
				var dbcsb = new DbConnectionStringBuilder() { { "Data Source", path } };
				ctx = new AssetInventoryDbContextSqlite() { ConnectionString_Sqlite = dbcsb.ConnectionString };
				ConnectOrDispose();
			}

			if (ctx is null)
				continueFlag = true;

		} while (continueFlag);

		return ctx;
	}

	private static string? SQLitePicker(bool pickNewFile = false)
	{
		OrderedChoiceList choices =
		[
			.. SqlitePathSuggestions()
				.Where(x =>
					(string)x.Data! == ":memory:" ||
					File.Exists((string)x.Data!) ^ pickNewFile
					),
			new(-2, "path", "Specify a path directly"),
			new(-3, "exit", "Return to previous menu"),
		];

		Console.WriteLine();
		do
		{
			OrderedChoice choice =
				pickNewFile ?
				choices.Prompt("Where would you like to create AssetInventory.db?") :
				choices.Prompt("Which AssetInventory.db would you like to open?");
			if (choice.Id == -2)
			{
				string path = ConsolePrompt.String("Enter direct path: ");
				if (path.IsNullOrEmpty())
				{
					Console.WriteLine("Returning to file picker.");
					continue;
				}
				return path;
			}

			if (choice.Id == -3)
				return null;

			return (string)choice.Data!;

		} while (true);
	}
	public static string? TryGetFolderPath(Environment.SpecialFolder folder)
	{
		try
		{
			return Environment.GetFolderPath(folder);
		}
		catch (PlatformNotSupportedException)
		{
		}
		return null;
	}
	public static string? TryGetTempPath()
	{
		try
		{
			return Path.GetTempPath();
		}
		catch (SecurityException)
		{
		}
		return null;
	}
	private static IEnumerable<OrderedChoice> SqlitePathSuggestions()
	{
		var basename = "AssetInventory.db";
		yield return new(-1, "memory", $"Sqlite in-memory volatile database")
		{
			Data = ":memory:"
		};
		yield return new(-1, "cd", $"The current directory ({Environment.CurrentDirectory})")
		{
			Data = basename
		};
		if (TryGetFolderPath(Environment.SpecialFolder.DesktopDirectory) is string desktop)
			yield return new(-1, "desktop", "The desktop folder")
			{
				Data = Path.Join(desktop, basename)
			};
		if (TryGetFolderPath(Environment.SpecialFolder.MyDocuments) is string documents)
			yield return new(-1, "documents", "The My Documents folder")
			{
				Data = Path.Join(documents, basename)
			};
		if (TryGetFolderPath(Environment.SpecialFolder.UserProfile) is string profile)
			yield return new(-1, "profile", "The folder above the desktop folder")
			{
				Data = Path.Join(profile, basename)
			};
		if (TryGetFolderPath(Environment.SpecialFolder.LocalApplicationData) is string AppData_Local)
			yield return new(-1, "local", "Typically AppData/Local/AssetInventory")
			{
				Data = Path.Join(AppData_Local, "AssetInventory/AssetInventory.db"),
				Hidden = true
			};
		if (TryGetFolderPath(Environment.SpecialFolder.ApplicationData) is string AppData_Roaming)
			yield return new(-1, "roaming", "Typically AppData/Roaming/AssetInventory")
			{
				Data = Path.Join(AppData_Roaming, "AssetInventory/AssetInventory.db"),
				Hidden = true
			};
		if (TryGetTempPath() is string temp)
			yield return new(-1, "temp", "Typically AppData/Local/Temp")
			{
				Data = Path.Join(temp, basename),
				Hidden = true
			};
	}
}