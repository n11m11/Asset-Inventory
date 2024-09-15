## About

This is a beginner terminal app written in C# which uses these dependencies:
- Entity Framework Core
- CsWin32/PInvoke
- Spectre.Console

This terminal app supports three different EF Core database providers:
- Microsoft SQL Server
- SQLite
- Entity Framework Core InMemory

![Short demonstration video](demo.avif)

## Running

Simply compile and run the program to get started and create a database.

It's possible to run multiple instances of the application at once even using SQLite.

If the `InMemory` database provider causes problems (it won't), use SQLite in-memory by navigating to `create` then `memory`.

## Connecting to a remote Microsoft SQL Server DBMS

Add the [connection string](https://learn.microsoft.com/en-us/ef/core/miscellaneous/connection-strings) to the configuration file.

Remember to configure appropriate permissions and authentication in SQL Server if you have problems connecting. Read-only configurations are not supported.

## Building

- Windows 10 or Windows 11
- Visual Studio Community 2022
- .NET 8 with C# 12

Non-Windows platforms have been considered in the code, but no testing has been done.

The project is about 90% complete.

## Migrations

The application creates the necessary databases at runtime as requested.

There is no need to run `Update-Database` unless the application is nonoperational.

New migrations are added using Entity Framework's `IDesignTimeDbContextFactory` using the following command:

```bat
Add-Migration -Context [context name] [migration name]
```

```sh
# for Linux
dotnet ef migrations add [migration name] --context [context name]
```

We do not provide support for `efbundle`.

## Configuration file

AssetInventory will look for `appsettings.json` in the _current working directory_ and load it upon startup. AssetInventory writes to this file. Use the interface to make configuration changes.

> [!WARNING]
> This JSON file may ***not*** contain comments.

```json
{
  "AutoConnect": {
    "Key": "Default",
    "Type": "sqlite"
  },
  "ConnectionStrings": {
    "Default" : "Data Source: path/to/folder/AssetInventory.db"
  },
  "ApiKey": ""
}
```

These values are valid for `AutoConnect.Type`:

```
"Type": "sqlite"
"Type": "sqlsever"
```

# Exchange Rate API

AssetInventory uses a database-backed cache to reduce the number of calls to the exchange rate API but it is still possible to hit this limit.
Please read their [Terms](https://www.exchangerate-api.com/terms) and add your API key in the configuration file as detailed above and restart the program to avoid being rate limited.

# Copyright

Copyright © 2024 n11m11
