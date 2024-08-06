//#define FUZZY_FIND_INCLUDED

namespace AssetInventory.Printing;

internal class FuzzyString
{
    static FuzzyString() {
#if !FUZZY_FIND_INCLUDED
#if DEBUG
        Console.WriteLine("DEBUG: INFO: Using the built-in fuzzy matcher.");
#endif
#endif
    }

	public static int Fuzzy(string needle, string haystack)
    {
#if FUZZY_FIND_INCLUDED
        return FuzzyTools.FuzzyFind(needle,haystack);
#else
        int lengthDifference = int.Clamp(int.Abs(haystack.Length - needle.Length), 0, 1);
        int commonLength = int.Min(needle.Length, haystack.Length);
        needle = needle.Substring(0, commonLength).ToLower();
        haystack = haystack.Substring(0, commonLength).ToLower();
        int maxLength = 12;
        if (commonLength > maxLength)
        {
            string SpliceMiddleToLength(string x, int l) => x.Length <= l ? x : x.Substring(0, l / 2) + x.Substring(x.Length - (l + 1) / 2, (l + 1) / 2);
            needle = SpliceMiddleToLength(needle, maxLength);
            haystack = SpliceMiddleToLength(haystack, maxLength);
        }
        return lengthDifference + Levenshtein.MemoizedLevenshtein(needle, haystack);
#endif
    }
    public static int LengthSensitiveFuzzy(string needle, string haystack)
    {
#if FUZZY_FIND_INCLUDED
        return FuzzyTools.FuzzyFind(needle,haystack);
#else
        return Levenshtein.MemoizedLevenshtein(needle, haystack);
#endif
    }
}