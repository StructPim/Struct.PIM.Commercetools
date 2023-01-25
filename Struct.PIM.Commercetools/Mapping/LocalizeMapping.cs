using System.Globalization;
using commercetools.Sdk.Api.Models.Common;
using Struct.PIM.Api.Models.Shared;
using Struct.PIM.Commercetools.Helpers;

namespace Struct.PIM.Commercetools.Mapping;

public static class LocalizeMapping
{
    public static LocalizedString Map(Dictionary<string, string> names)
    {
        var localizedString = new LocalizedString();

        foreach (var entry in names)
        {
            try
            {
                var langName = CultureInfo.GetCultureInfo(entry.Key).TwoLetterISOLanguageName;
                localizedString.Add(langName, entry.Value ?? "");
            }
            catch (Exception)
            {
                // ignored
            }
        }


        return localizedString;
    }

    public static LocalizedString? Map(List<LocalizedData<string>>? localizedDatas)
    {
        var localizedString = new LocalizedString();
        if (localizedDatas == null)
        {
            return null;
        }

        foreach (var entry in localizedDatas)
        {
            try
            {
                var langName = LocalizeHelper.GetTwoLetterIsoLanguageName(entry.CultureCode);
                localizedString.Add(langName, entry.Data ?? "");
            }
            catch (Exception)
            {
                // ignored
            }
        }

        return localizedString;
    }
}