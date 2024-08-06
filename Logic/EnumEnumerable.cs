using System.Collections;

namespace AssetInventory.Logic;

/// <summary>
/// Convert an enum to an enumerable over all values.
/// </summary>
/// <typeparam name="T">The enum.</typeparam>
public class EnumEnumerable<T> : IEnumerable<T> where T : Enum
{
    public IEnumerator<T> GetEnumerator()
    {
        return Enum.GetValues(typeof(T)).Cast<T>().GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}