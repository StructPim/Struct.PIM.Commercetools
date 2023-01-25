using commercetools.Sdk.Api.Models.Categories;
using Struct.PIM.Api.Models.Catalogue;
using Struct.PIM.Api.Models.Product;
using Struct.PIM.Commercetools.Mapping;

namespace Struct.PIM.Commercetools.Helpers;

public static class CategoryHelper
{
    public static CategoryUpdate? GetChanges(this CategoryModel updated, ICategory? existing, ICategory? parent)
    {
        if (existing == null)
        {
            return null;
        }

        var categoryModelMapped = updated.Map();
        var actions = GetChanges(existing, categoryModelMapped);

        if (parent != null && updated.ParentId != null && parent.Key != updated.ParentId.ToCommerceKey())
        {
            actions.Add(new CategoryChangeParentAction
            {
                Parent = new CategoryResourceIdentifier { Key = updated.ParentId.ToCommerceKey() }
            });
        }

        return GetUpdate(actions, existing.Version);
    }

    public static CategoryUpdate? GetChanges(this CatalogueModel updated, ICategory? existing)
    {
        if (existing == null)
        {
            return null;
        }

        var categoryModelMapped = updated.Map();
        var actions = GetChanges(existing, categoryModelMapped);
        return GetUpdate(actions, existing.Version);
    }

    /// <summary>
    ///     Handle PIM classifications. If no primary category is defined, a primary category will be chosen from the first
    ///     element from the list of classifications
    /// </summary>
    /// <param name="productClassificationModels"></param>
    public static Tuple<int?, List<int>?> HandleClassification(this List<ProductClassificationModel> productClassificationModels)
    {
        var primaryClassificationId = productClassificationModels?.FirstOrDefault(p => p.IsPrimary)?.CategoryId;
        var nonPrimaryClassifications = productClassificationModels?.Where(p => !p.IsPrimary).Select(p => p.CategoryId).ToList();
        if (primaryClassificationId != null)
        {
            return new Tuple<int?, List<int>?>(primaryClassificationId, nonPrimaryClassifications);
        }

        primaryClassificationId = nonPrimaryClassifications?.FirstOrDefault();
        if (primaryClassificationId != null)
        {
            nonPrimaryClassifications?.Remove(primaryClassificationId.Value);
        }

        return new Tuple<int?, List<int>?>(primaryClassificationId, nonPrimaryClassifications);
    }

    private static List<ICategoryUpdateAction> GetChanges(ICategory existing, ICategory updated)
    {
        var actions = new List<ICategoryUpdateAction>();

        if (existing.Name.HasChanges(updated.Name))
        {
            actions.Add(new CategoryChangeNameAction { Name = updated.Name });
        }

        if (existing.Slug.HasChanges(updated.Slug))
        {
            actions.Add(new CategoryChangeSlugAction { Slug = updated.Slug });
        }

        return actions;
    }

    private static CategoryUpdate? GetUpdate(List<ICategoryUpdateAction> actions, long version)
    {
        return actions.Count > 0
            ? new CategoryUpdate
            {
                Version = version,
                Actions = actions
            }
            : null;
    }
}