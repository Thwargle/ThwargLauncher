using System;
using System.Text;

namespace ThwargleStringExtensions
{
    public static class StringUtil
    {
        public static string Limit(string text, int maxlen = 10)
        {
            if (text == null || maxlen < 1) { return ""; }
            if (text.Length <= maxlen) { return text; }
            if (maxlen < 4) { return text.Substring(0, maxlen); }
            return text.Substring(0, maxlen - 2) + "..";
        }
    }
    public static class StringUtilExtensions
    {
        public static string Limit(this string text, int maxlen = 10) { return StringUtil.Limit(text, maxlen); }
    }
}
