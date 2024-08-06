using System.Text.RegularExpressions;

namespace AssetInventory.HttpConnectivity;

//{
//	"result": "success",
//	"time_next_update_unix": 1585959987,
//	"time_next_update_utc": "Sat, 03 Apr 2020 00:26:27 +0000",
//	"time_eol_unix": 0,
//	"base_code": "USD",
//	"rates": {
//		"USD": 1,
//		"AED": 3.67,
//		"ARS": 64.51,
//		"AUD": 1.65,
//		"CAD": 1.42,
//		"...": 7.85,
//		"...": 1.31,
// Read the documentation for information about how to use their API.

static partial class RegexJsonUtils
{
	internal static string number_from_re_key(string haystack, string re_key)
	{
		return new Regex($"""(?:"{re_key}")\s*:\s*([-.+0-9]+(?:e[-+][0-9]+)?\b)""").Match(haystack).Groups[1].Value;
	}

	internal static double double_from_re_key(string json, string re_key) => double.Parse(number_from_re_key(json,re_key));

	internal static double time_next_update_unix(string json) => double_from_re_key(json,"time_next_update_unix");

	internal static string rates_from_json(string json) => re_rates_from_json().Match(json).Groups[1].Value;

	[GeneratedRegex("""(?:"rates")\s*:\s*({[\s\w\d.,+":-]*})""")]
	private static partial Regex re_rates_from_json();
}