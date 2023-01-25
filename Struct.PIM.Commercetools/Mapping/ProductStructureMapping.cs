using commercetools.Sdk.Api.Models.ProductTypes;
using Struct.PIM.Api.Models.ProductStructure;
using Struct.PIM.Commercetools.Helpers;
using Attribute = Struct.PIM.Api.Models.Attribute.Attribute;

namespace Struct.PIM.Commercetools.Mapping;

public static class ProductTypeMapping
{
    public static ProductTypeDraft MapDraft(this ProductStructure productStructure, List<Attribute>? productAttributes, List<Attribute>? variantAttributes, List<string>? includeAliases = null)
    {
        var draft = new ProductTypeDraft
        {
            Name = productStructure.Label,
            Description = productStructure.Alias,
            Key = productStructure.Uid.ToCommerceKey()
        };
        if (productAttributes == null)
        {
            return draft;
        }

        var skuList = new List<string> { "sku" };
        draft.Attributes = productAttributes.MapDraft(IAttributeConstraintEnum.SameForAll, skuList, includeAliases).ToList();
        if (variantAttributes == null)
        {
            return draft;
        }

        includeAliases ??= skuList;
        if (!includeAliases.Contains("sku"))
        {
            includeAliases.Add("sku");
        }

        includeAliases = includeAliases.ConvertAll(p => p.ToLower());
        draft.Attributes = draft.Attributes.Concat(variantAttributes.MapDraft(IAttributeConstraintEnum.Unique, null, includeAliases))
            .GroupBy(p => p.Name)
            .Select(q => q.First()).ToList();
        return draft;
    }

    public static ProductType Map(this ProductStructure productStructure, List<Attribute>? productAttributes, List<Attribute>? variantAttributes, List<string>? includeAliases = null)
    {

        var productType = new ProductType
        {
            Name = productStructure.Alias,
            Description = productStructure.Label,
            Key = productStructure.Uid.ToCommerceKey()
        };

        if (productAttributes == null)
        {
            return productType;
        }

        var skuList = new List<string> { "sku" };
        productType.Attributes = productAttributes.Map(IAttributeConstraintEnum.SameForAll, skuList, includeAliases).ToList();
        if (variantAttributes == null)
        {
            return productType;
        }

        includeAliases ??= skuList;
        if (!includeAliases.Contains("sku"))
        {
            includeAliases.Add("sku");
        }

        includeAliases = includeAliases.ConvertAll(p => p.ToLower());
        productType.Attributes = productType.Attributes.Concat(variantAttributes.Map(IAttributeConstraintEnum.Unique, null, includeAliases))
            .GroupBy(x => x.Name)
            .Select(x => x.First()).ToList();


        return productType;

    }
}