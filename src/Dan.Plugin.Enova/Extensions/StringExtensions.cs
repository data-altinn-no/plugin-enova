using System.Text;

namespace Dan.Plugin.Enova.Extensions;

public static class StringExtensions
{
    // https://www.codeproject.com/Articles/1014073/Fastest-method-to-remove-all-whitespace-from-Strin
    // Removing white space in the middle of a string is surprisingly not as straight forward as one might think
    // The article above goes into detail of it and where this method is found from
    public static string TrimAllWhitespace(this string s) {
        var length = s.Length;
        var buffer = new StringBuilder(s);
        var dstIdx = 0;
        foreach (var ch in s)
        {
            switch (ch) {
                case '\u0020': case '\u00A0': case '\u1680': case '\u2000': case '\u2001':
                case '\u2002': case '\u2003': case '\u2004': case '\u2005': case '\u2006':
                case '\u2007': case '\u2008': case '\u2009': case '\u200A': case '\u202F':
                case '\u205F': case '\u3000': case '\u2028': case '\u2029': case '\u0009':
                case '\u000A': case '\u000B': case '\u000C': case '\u000D': case '\u0085':
                    length--;
                    continue;
            }
            buffer[dstIdx++] = ch;
        }
        buffer.Length = length;
        return buffer.ToString();;
    }
}
