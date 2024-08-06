using System.Text.Json;
using AssetInventory.Data;

namespace Asset_Inventory.JsonConfig;

internal class JsonRepository
{
	public string Filename = "appsettings.json";
	public string FilePath => Path.Join(Environment.CurrentDirectory, Filename);

	WeakReference<AppSettingsSchema> weakRefJson = new(null!);

	/// <summary>
	/// This property automatically reads and writes to the config file when set or get.
	/// </summary>
	AppSettingsSchema Json
	{
		get
		{
			if (!weakRefJson.TryGetTarget(out AppSettingsSchema Json))
			{
#if DEBUG
                Console.WriteLine("DEBUG: WeakReference<AppSettingsSchema> is new instance.");
#endif
				//StreamReader reader = new StreamReader(FilePath);
				//reader.ReadToEnd();
				using (var f = File.Open(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					Json = (AppSettingsSchema)f;
				}
				weakRefJson.SetTarget(Json);
			}
			return Json;
		}
		set
		{
			weakRefJson.SetTarget(value);
			FileStream _f;
			if (File.Exists(FilePath))
				_f = File.Open(FilePath, FileMode.OpenOrCreate | FileMode.Truncate, FileAccess.ReadWrite);
			else
				_f = File.Open(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
			using (var f = _f)
			{
				Utf8JsonWriter writer = new(f);
				JsonSerializer.Serialize(writer, value, JsonSerializerOptions.Default);
			}
		}
	}
	AppSettingsSchema JsonOrNew
	{
		get
		{
			if (!File.Exists(FilePath))
				return new();
			return Json;
		}
	}







	public void TryReading()
	{
		if (new FileInfo(FilePath).Length == 0)
			throw new EmptyConfigException();
		_ = Json;
	}

	public void Create()
	{
		if (!File.Exists(FilePath) || new FileInfo(FilePath).Length == 0)
			Json = new();
	}

	public string GetApiKey()
	{
		return JsonOrNew.ApiKey ?? "";
	}

	public IEnumerable<KeyValuePair<string, string>> GetAllConnectionStrings()
	{
		return JsonOrNew.ConnectionStrings ??= new();
	}
	public string? GetConnectionString()
	{
		string key = JsonOrNew.AutoConnect?.Key ?? "Default";
		return GetConnectionString(key);
	}
	public string? GetConnectionString(string key)
	{
		string? value;
		if (JsonOrNew.ConnectionStrings is var d)
			d.TryGetValue(key, out value);
		return value;
	}

	public void SetConnectionString(string key, string value)
	{
		var json = JsonOrNew;
		json.ConnectionStrings ??= new();
		json.ConnectionStrings[key] = value;
		Json = json;
	}
	public void RemoveConnectionString(string key)
	{
		var json = JsonOrNew;
		json.ConnectionStrings ??= new();
		json.ConnectionStrings.Remove(key);
		Json = json;
	}

	public void SetAutoConnect(string key, string type)
	{
		key ??= "Default";
		type ??= AutoConnectSchema.Types[0];
		var json = JsonOrNew;
		json.AutoConnect ??= new();
		json.AutoConnect.Key = key;
		json.AutoConnect.Type = type;
		Json = json;
	}
	public AutoConnectSchema GetAutoConnect()
	{
		return JsonOrNew?.AutoConnect ?? new();
	}

	public AssetInventoryDbContext? GetCtx()
	{
		var ac = GetAutoConnect();
		if (string.IsNullOrEmpty(ac.Key))
			return null; // no error
		return GetConnectionString(ac.Key) switch
		{
			string cs => ac.Type?.ToLower() switch
			{
				"sqlserver" => new AssetInventoryDbContextSqlServer()
				{
					ConnectionString_SqlServer = cs
				},
				"sqlite" => new AssetInventoryDbContextSqlite()
				{
					ConnectionString_Sqlite = cs
				},
				_ => throw new InvalidConfigException()
			},
			_ => throw new InvalidConfigException()
		};
	}

}
