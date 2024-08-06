## About

This is a beginner terminal app written in C# which uses these dependencies:
- Entity Framework Core
- CsWin32/PInvoke
- Spectre.Console

This terminal app supports three different EF Core database providers:
- InMemory
- SqlServer
- SQLite

![Short demonstration video](demo.avif)

If the `InMemory` database provider causes problems, use SQLite in-memory by navigating to `create` then `memory`.

The project is about 90% complete.

## Running

Simply compile and run the program to get started and create a database.

It's possible to run multiple instances of the application at once even using SQLite. Remember to configure permissions and authentication in SQL Server for your users. Read-only configurations are not supported.

## Connecting to a remote Microsoft SQL Server DBMS

Add the [connection string](https://learn.microsoft.com/en-us/ef/core/miscellaneous/connection-strings) to the configuration file.

## Building

Visual Studio Community 2022
Windows 10 or Windows 11
.NET 8 with C# 12

Non-Windows platforms have been considered in the code, but no testing has been done.

## Migrations

The application creates the necessary databases at runtime as requested.
There is no need to run `Update-Database` unless the application is nonoperational.

New migrations are added using `IDesignTimeDbContextFactory` using the following commands:

```bat
Add-Migration -Context [context name] [migration name]
```

```sh
# Linux
dotnet ef migrations add [migration name] --context [context name]
```

We do not provide support for `efbundle`.

## Configuration file

AssetInventory will look for `appsettings.json` in the _current working directory_ and load it upon startup.
> [!WARNING]
> JSON files may ***not*** contain comments.

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

# Exchange rate API

AssetInventory uses a database-backed cache to reduce the number of calls to the exchange rate API but it is still possible to hit this limit.
Please read their [Terms](https://www.exchangerate-api.com/terms) and add your API key in the configuration file as detailed above and restart the program to avoid being rate limited.

# Copyright

Copyright © 2024 n11m11
