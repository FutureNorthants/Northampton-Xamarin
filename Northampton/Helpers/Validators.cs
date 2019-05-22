using System;
using System.Text.RegularExpressions;

namespace Northampton
{
    public static class Validators
    {
        public static Boolean IsValidPostcode(string postcode)
        {
            Regex regex = new Regex(@"([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9][A-Za-z]?))))\s?[0-9][A-Za-z]{2})");
            Match match = regex.Match(postcode);
            if (match.Success)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static Boolean IsValidAreaPostcode(string postcode)
        {
            if (postcode.ToLower().StartsWith("nn", StringComparison.Ordinal))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
