namespace Struct.PIM.Commercetools.Helpers;

public static class IdHelper
{
    private static readonly string _prefix = "struct_";

    public static string? ToCommerceKey(this int? structId)
    {
        return structId.HasValue ? ToCommerceKey(structId.Value) : null;
    }

    public static string ToCommerceKey(this int structId)
    {
        return $"{_prefix}{structId}".ToLower();
    }

    public static string ToCommerceKey(this string structId)
    {
        return structId.ToLower();
    }

    public static string ToCommerceKey(this Guid uid)
    {
        return uid.ToString().ToLower();
    }

    public static int ToStructId(this string commerceKey)
    {
        return int.TryParse(commerceKey.Substring(commerceKey.IndexOf(_prefix, StringComparison.Ordinal)), out var id) ? id : -1;
    }
}