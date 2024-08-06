using System.Security.Cryptography;
using Spectre.Console;

namespace AssetInventory.Printing;

internal static class ColorHash
{
    /// <summary>
    /// Converts text to a Color. Biased to produce lighter colors.
    /// </summary>
    internal static Color QuickHash(string s)
    {
        uint q = s.AsEnumerable().Aggregate(uint.MaxValue, (a, u16_ch) => a * (u16_ch ^ a >> 24) ^ u16_ch) >> 8;
        uint max = uint.Max(q & 255, uint.Max(q >> 8 & 255, q >> 16 & 255));
        byte n(uint x) => (byte)(x % 256 * 255 / max);
        return max == 0 ? Color.Grey : new Color(n(q), n(q >> 8), n(q >> 16));
    }
}