using System.Globalization;
using commercetools.Sdk.Api.Models.Common;

namespace Struct.PIM.Commercetools.Helpers;

public static class LocalizeHelper
{
    public static string GetTwoLetterIsoLanguageName(string cultureCode)
    {
        return CultureInfo.GetCultureInfo(cultureCode).TwoLetterISOLanguageName;
    }

    public static LocalizedString GetSlug(LocalizedString? localizedString, string key)
    {
        var slug = new LocalizedString();
        if (localizedString == null)
        {
            return slug;
        }

        localizedString.Keys.ToList().ForEach(p => slug.Add(p, key.RemoveSpaces()));
        return slug;
    }
}