namespace Struct.PIM.Commercetools;

public static class Settings
{
    public static string? CommerceProjectKey { get; set; }
    public static string? StructBaseUrl { get; set; }
    public static string? StructApiKey { get; set; }

    public static void SetCurrentCommerceProjectKey(string? projectKey)
    {
        CommerceProjectKey = projectKey;
    }

    public static void SetStructBaseurl(string? baseUrl)
    {
        StructBaseUrl = baseUrl;
    }

    public static void SetStructApiKey(string? apiKey)
    {
        StructApiKey = apiKey;
    }
}