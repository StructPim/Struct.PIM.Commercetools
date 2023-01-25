using System.Text.Json;
using commercetools.Sdk.Api.Client.RequestBuilders.Projects;
using commercetools.Sdk.Api.Models.Common;
using commercetools.Sdk.Api.Models.CustomObjects;
using Struct.PIM.Api.Models.Shared;
using Struct.PIM.Commercetools.Mapping;

namespace Struct.PIM.Commercetools.Helpers;

public static class AttributeHelper
{
    public static dynamic? ResolveValue(ByProjectKeyRequestBuilder builder, JsonElement? attribute, string? key = null)
    {
        if (attribute == null)
        {
            return null;
        }

        var attributeVal = attribute.Value;
        switch (attributeVal.ValueKind)
        {
            case JsonValueKind.Array:
                try
                {
                    var val = LocalizeMapping.Map(attributeVal.Deserialize<List<LocalizedData<string>>>());
                    return val?.Count > 0 ? val : null;
                }
                catch (Exception)
                {
                    return CreateCustomObjectReference(builder, attributeVal, key);
                }
            case JsonValueKind.False:
            case JsonValueKind.True:
                return attributeVal.Deserialize<bool>();
            case JsonValueKind.Number:
                return attributeVal.Deserialize<decimal>();
            case JsonValueKind.String:
                return attributeVal.Deserialize<string>() ?? "";
            case JsonValueKind.Object:
                return CreateCustomObjectReference(builder, attributeVal, key);
            case JsonValueKind.Undefined:
                break;
            case JsonValueKind.Null:
                break;
        }

        return new object();
    }

    private static CustomObjectReference CreateCustomObjectReference(ByProjectKeyRequestBuilder builder, JsonElement attribute, string? key)
    {
        var container = "container_" + key;
        key = "key_" + key;
        ICustomObject? customObject;
        try
        {
            customObject = builder.CustomObjects().WithContainerAndKey(container, key).Get().ExecuteAsync().Result;
        }
        catch (Exception)
        {
            var customObjectDraft = new CustomObjectDraft
            {
                Version = 0,
                Key = key,
                Container = container,
                Value = attribute.ToString()
            };
            customObject = builder.CustomObjects().Post(customObjectDraft).ExecuteAsync().Result;
        }

        return new CustomObjectReference
            { Id = customObject.Id, TypeId = IReferenceTypeId.KeyValueDocument };
    }
}