namespace AssetInventory.HttpConnectivity;

internal record ApiResponseRecord(string? Uri, HttpResponseMessage Message, string Body);

interface IApiCacheInterface<TKey, TEntity>
{
	public TEntity? Cast(TKey key, ApiResponseRecord apiResponseRecord);
	public TEntity? CacheFind(TKey key);
	public void CacheSet(TKey key, TEntity entity);
	public bool CacheCheckNotExpired(TEntity entity);
}

internal class ApiCacher<TKey, TEntity>(HttpClient client, IApiCacheInterface<TKey, TEntity> proxy) where TEntity : class
{
	HttpClient client = client;
	IApiCacheInterface<TKey, TEntity> proxy = proxy;

	public long api_hits = 0;
	public long cache_hits = 0;
	public long cache_misses = 0;
	public long cache_expiries = 0;

	ApiResponseRecord HttpGetUri(string uri)
	{
		api_hits++;
		return Task.Run<ApiResponseRecord>(async () => await HttpGetUriAsync(uri)).GetAwaiter().GetResult();
	}

	async Task<ApiResponseRecord> HttpGetUriAsync(string? uri)
	{
		HttpResponseMessage responseMsg = await client.GetAsync(uri);
		responseMsg.EnsureSuccessStatusCode();
		string body = await responseMsg.Content.ReadAsStringAsync();
		return new(uri, responseMsg, body);
	}

	public TEntity? GetUri(TKey key, string? uri = null)
	{
		ApiResponseRecord response = HttpGetUri(uri);
		if (proxy.Cast(key, response) is TEntity entity)
		{
			proxy.CacheSet(key, entity);
			return entity;
		}
		return null;
	}

	public TEntity? FindOrGetUri(TKey key, string? uri, bool allowExpired = false)
	{
		cache_misses++;
		if (proxy.CacheFind(key) is TEntity body)
		{
			cache_misses--;
			cache_hits++;
			cache_expiries++;
			if (allowExpired || proxy.CacheCheckNotExpired(body))
			{
				cache_expiries--;
				return body;
			}
		}

		return GetUri(key, uri);
	}
}