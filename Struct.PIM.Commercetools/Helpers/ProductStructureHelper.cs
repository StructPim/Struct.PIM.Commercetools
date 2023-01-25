using commercetools.Sdk.Api.Models.ProductTypes;
using Struct.PIM.Api.Models.DataConfiguration;
using Struct.PIM.Api.Models.ProductStructure;
using Struct.PIM.Commercetools.Mapping;
using Attribute = Struct.PIM.Api.Models.Attribute.Attribute;

namespace Struct.PIM.Commercetools.Helpers;

public static class ProductStructureHelper
{
    public static ProductTypeUpdate? GetChanges(this ProductStructure updated, IProductType? existing, List<Attribute>? productAttributes, List<Attribute>? variantAttributes)
    {
        if (existing == null)
        {
            return null;
        }

        var productTypeMapped = updated.Map(productAttributes, variantAttributes);
        var actions = GetChanges(existing, productTypeMapped);
        return GetUpdate(actions, existing.Version);
    }

    private static List<IProductTypeUpdateAction> GetChanges(IProductType existing, IProductType updated)
    {
        var actions = new List<IProductTypeUpdateAction>();

        if (existing.Name.HasChanges(updated.Name))
        {
            actions.Add(new ProductTypeChangeNameAction { Name = updated.Name });
        }

        if (existing.Description.HasChanges(updated.Description))
        {
            actions.Add(new ProductTypeChangeDescriptionAction
                { Description = updated.Description });

        }

        // For now we don't handle updating the attribute, since the Commercetools API is very limited

        return actions;
    }

    private static ProductTypeUpdate? GetUpdate(List<IProductTypeUpdateAction> actions, long version)
    {
        return actions.Count > 0
            ? new ProductTypeUpdate
            {
                Version = version,
                Actions = actions
            }
            : null;
    }


    public static IEnumerable<Guid> GetProductAttributeUids(ProductStructure productStructure)
    {
        return GetAttributeGuids(productStructure.ProductConfiguration.Tabs);
    }

    public static IEnumerable<Guid> GetVariantAttributeUids(ProductStructure productStructure)
    {
        return GetAttributeGuids(productStructure.VariantConfiguration.Tabs);

    }


    private static IEnumerable<Guid> GetAttributeGuids(IEnumerable<TabSetup> tabSetups)
    {
        return tabSetups.Where(x => x.GetType() == typeof(DynamicTabSetup))
            .Cast<DynamicTabSetup>()
            .SelectMany(p => p.Sections.Where(q => q.GetType() == typeof(DynamicSectionSetup)))
            .Cast<DynamicSectionSetup>()
            .SelectMany(q => q.Properties.Where(u => u.GetType() == typeof(AttributeSetup)))
            .Cast<AttributeSetup>()
            .Select(p => p.AttributeUid);
    }

    public static bool VariantAttributeInherits(this ProductStructure productStructure, Guid attributeUid)
    {
        return productStructure.VariantConfiguration.Tabs.Where(x => x.GetType() == typeof(DynamicTabSetup))
                   .Cast<DynamicTabSetup>()
                   .SelectMany(p => p.Sections.Where(q => q.GetType() == typeof(DynamicSectionSetup)))
                   .Cast<DynamicSectionSetup>()
                   .SelectMany(q => q.Properties.Where(u => u.GetType() == typeof(AttributeSetup)))
                   .Cast<AttributeSetup>()
                   .FirstOrDefault(p => p.AttributeUid == attributeUid)?.Inherits ??
               false;
    }
}