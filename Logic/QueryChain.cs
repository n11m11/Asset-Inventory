namespace AssetInventory.Logic;

/// <summary>
/// Intended for use with IQueryable.
/// </summary>
/// <typeparam name="T">IQueryable</typeparam>
/// <param name="getStartingQuery">A Func&lt;IQueryable&gt; used to reset the query</param>
internal class QueryChain<T>(Func<T> getStartingQuery)
{
	public int FilterCount = 0;
	private Func<T> getInitialQuery = getStartingQuery;
	public T AggregateQuery = getStartingQuery();

	public void Reset()
	{
		FilterCount = 0;
		AggregateQuery = getInitialQuery();
	}
	public QueryChain<T> Add(Func<T, T> x)
	{
		AggregateQuery = x(AggregateQuery);
		FilterCount++;
		return this;
	}
}