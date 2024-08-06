using Asset_Inventory.ExtensionMethods;
using AssetInventory.Data;
using AssetInventory.Models;
using Microsoft.EntityFrameworkCore;
using static AssetInventory.HttpConnectivity.Currency;

namespace AssetInventory.HttpConnectivity;

internal class ExchangeRateApiRepository : CacheProxy
{
	public static readonly IList<Currency> SupportedCurrencies = [
		AED, AFN, ALL, AMD, ANG, AOA, ARS, AUD, AWG, AZN, BAM, BBD, BDT, BGN, BHD, BIF, BMD, BND, BOB, BRL,
		BSD, BTN, BWP, BYN, BZD, CAD, CDF, CHF, CLP, CNY, COP, CRC, CUP, CVE, CZK, DJF, DKK, DOP, DZD, EGP,
		ERN, ETB, EUR, FJD, FKP, FOK, GBP, GEL, GGP, GHS, GIP, GMD, GNF, GTQ, GYD, HKD, HNL, HRK, HTG, HUF,
		IDR, ILS, IMP, INR, IQD, IRR, ISK, JEP, JMD, JOD, JPY, KES, KGS, KHR, KID, KMF, KRW, KWD, KYD, KZT,
		LAK, LBP, LKR, LRD, LSL, LYD, MAD, MDL, MGA, MKD, MMK, MNT, MOP, MRU, MUR, MVR, MWK, MXN, MYR, MZN,
		NAD, NGN, NIO, NOK, NPR, NZD, OMR, PAB, PEN, PGK, PHP, PKR, PLN, PYG, QAR, RON, RSD, RUB, RWF, SAR,
		SBD, SCR, SDG, SEK, SGD, SHP, SLE, SOS, SRD, SSP, STN, SYP, SZL, THB, TJS, TMT, TND, TOP, TRY, TTD,
		TVD, TWD, TZS, UAH, UGX, USD, UYU, UZS, VES, VND, VUV, WST, XAF, XCD, XDR, XOF, XPF, YER, ZAR, ZMW,
		ZWL
	];

	public ApiCacher<Currency, HttpCache> CacheCtx;
	internal string? ApiKey;

	public ExchangeRateApiRepository(AssetInventoryDbContext ctx) : base(ctx)
	{
		//new HttpClientHandler().
		HttpClient httpClient;
		httpClient = new HttpClient()
		{
			BaseAddress = new Uri("https://open.er-api.com/v6/latest/EUR") // GET
		};
		if (!string.IsNullOrWhiteSpace(ApiKey))
		{
			httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
		}
		CacheCtx = new(httpClient, this);
	}

	internal void RemoveAll()
	{
		if (ctx.Database.IsRelational())
			ctx.HttpCaches.ExecuteDelete();
		else
			foreach(var x in ctx.HttpCaches)
				ctx.HttpCaches.Remove(x);
		ctx.SaveChangesWithStatusAndLog();
	}

	internal IEnumerable<Currency>? GetCachedKeys()
	{
		return ctx.HttpCaches.Select(x => Enum.Parse<Currency>(x.Key));
	}

	internal double GetRate(Currency from, Currency to)
	{
		var json = CacheCtx.FindOrGetUri(from,from.ToString())?.Body;
		string rates = RegexJsonUtils.rates_from_json(json);
		return RegexJsonUtils.double_from_re_key(rates, to.ToString());
	}
}


class CacheProxy(AssetInventoryDbContext ctx) : IApiCacheInterface<Currency, HttpCache>
{
	protected readonly AssetInventoryDbContext ctx = ctx;



	public HttpCache? CacheFind(Currency key)
	{
		return ctx.HttpCaches.Find(key.ToString());
	}

	public HttpCache Cast(Currency key, ApiResponseRecord apiResponseRecord)
	{
		double unixValidTo;
		try
		{
			unixValidTo = RegexJsonUtils.time_next_update_unix(apiResponseRecord.Body);
		}
		catch
		{
			unixValidTo = 0;
		}

		return new HttpCache()
		{
			Key = key.ToString(),
			Body = apiResponseRecord.Body,
			UnixValidTo = unixValidTo,
		};
	}

	public void CacheSet(Currency key, HttpCache entity)
	{
		if (CacheFind(key) is HttpCache entity_org)
		{
			entity_org.Body = entity.Body;
			entity_org.UnixValidTo = entity.UnixValidTo;
		}
		else
			ctx.HttpCaches.Add(entity);
		ctx.SaveChangesWithStatusAndLog();
	}

	public bool CacheCheckNotExpired(HttpCache entity)
	{
		return entity.UnixValidTo == 0 ? true : (DateTime.UtcNow.Subtract(DateTime.UnixEpoch)).TotalSeconds < entity.UnixValidTo;
	}
}