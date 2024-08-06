using System.Text.Json;

namespace Asset_Inventory.JsonConfig;

internal class AppSettingsSchema
{
    public static explicit operator AppSettingsSchema(Stream stream)
    {
        return JsonSerializer.Deserialize<AppSettingsSchema>(stream);
    }


    public AutoConnectSchema AutoConnect { get; set; }
    public Dictionary<string, string> ConnectionStrings { get; set; }
    public string ApiKey { get; set; }
}

public class AutoConnectSchema
{
    public string Key { get; set; }
    public string Type { get; set; }

    public readonly static string[] Types = [
        "sqlite",
        "sqlserver",
    ];
}
