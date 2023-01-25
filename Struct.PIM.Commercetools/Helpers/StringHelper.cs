using System.Text.RegularExpressions;

namespace Struct.PIM.Commercetools.Helpers;

public static class StringHelper
{
    public static string RemoveSpaces(this string str)
    {

        return string.IsNullOrEmpty(str) ? "" : Regex.Replace(str, @"\s+", "");
    }
}