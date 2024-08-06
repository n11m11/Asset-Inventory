namespace AssetInventory.Printing;

internal class Levenshtein
{
    public static int MemoizedLevenshtein(string a, string b)
    {
        Dictionary<(string a, string b), int> cache = new();
        int L(string a, string b)
        {
            while (a.Length > 0 && b.Length > 0 && a[0] == b[0])
            {
                a = a.Substring(1);
                b = b.Substring(1);
            }
            if (a.Length == 0) return b.Length;
            if (b.Length == 0) return a.Length;

            if (cache.ContainsKey((a, b)))
                return cache[(a, b)];
            int X = L(a.Substring(1), b.Substring(1));
            int Y = X == 0 ? 0 : L(a, b.Substring(1));
            int Z = Y == 0 ? 0 : L(a.Substring(1), b);
            int distance = 1 + int.Min(X, Z < Y ? Z : Y);
            cache.Add((a, b), distance);
            return distance;
        }
        return L(a, b);
    }
}