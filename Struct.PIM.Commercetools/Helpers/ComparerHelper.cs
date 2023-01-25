using commercetools.Sdk.Api.Models.Common;

namespace Struct.PIM.Commercetools.Helpers;

public static class ComparerHelper
{
    public static bool HasChanges(this ILocalizedString existing, ILocalizedString? changed)
    {
        return changed != null &&
               (!existing.Keys.Any(changed.ContainsKey) ||
                existing.Any(entry => changed[entry.Key] != entry.Value));
    }

    public static bool HasChanges(this string existing, string? changed)
    {
        return changed != null && existing != changed;
    }
}