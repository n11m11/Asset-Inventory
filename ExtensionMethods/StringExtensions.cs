using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asset_Inventory.ExtensionMethods;

internal static class StringExtensions
{
    public static string LeftEllipsis(this string s, uint l)
    {
        return s.Length <= l ? s : "..." + s.Substring(s.Length - (int)l + 3).Trim();
    }
    public static string RightEllipsis(this string s, uint l)
    {
        return s.Length <= l ? s : (s.Substring(0, int.Max(0, (int)l - 3)).Trim() + "...").Slice(0, (int)l);
    }
    public static string Slice(this string s, int o, int l)
    {
        if (o < 0)
            o = int.Max(s.Length + o, 0);
        if (l < 0)
            l = int.Max(s.Length + l, 0);
        s = s.Substring(int.Min(s.Length, o));
        return s.Substring(0, int.Min(s.Length, l));
    }
}
