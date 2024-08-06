namespace AssetInventory.Logic;

/// <summary>
/// <![CDATA[
/// Memoizer wraps a Dictionary<TKey, TValue> to memoize the output of a Func<TKey, TValue>.
/// ]]>
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
/// <param name="value">The function to memoize.</param>
internal class Memoizer<TKey, TValue>(Func<TKey, TValue> value) where TKey : notnull
{
	Func<TKey, TValue> value = value;
	protected Dictionary<TKey, TValue> cache = new();
	public TValue this[TKey key]
	{
		get => cache.ContainsKey(key) ?
			   cache[key] :
			   cache[key] = value(key);
		set => cache[key] = value;
	}
}
/// <summary>
/// <![CDATA[
/// Memoizer wraps a Dictionary<TKey, TValue> to memoize the output of a Func<TSubscript, TValue>.
/// ]]>
/// </summary>
/// <typeparam name="TSubscript"></typeparam>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
/// <param name="value">This function converts the subscript to a value to be memoized.</param>
/// <param name="key">This function converts the subscript to an appropriate dictionary key.</param>
internal class Memoizer<TSubscript, TKey, TValue>(Func<TSubscript, TValue> value, Func<TSubscript, TKey> key) where TKey : notnull
{
	Func<TSubscript, TValue> value = value;
	protected Dictionary<TKey, TValue> cache = new();
	public TValue this[TSubscript sub]
	{
		get => cache.ContainsKey(key(sub)) ?
			   cache[key(sub)] :
			   cache[key(sub)] = value(sub);
		set => cache[key(sub)] = value;
	}
}