using commercetools.Sdk.Api.Models.Categories;
using commercetools.Sdk.Api.Models.Common;
using Struct.PIM.Api.Models.Catalogue;
using Struct.PIM.Commercetools.Helpers;
using Struct.PIM.WebhookModels;

namespace Struct.PIM.Commercetools.Mapping;

public static class CategoryMapping
{
    /// <summary>
    ///     Maps a Struct CatalogueWebhookModel to a CatalogueModel
    /// </summary>
    /// <param name="catalogueWebhookModel"></param>
    public static CatalogueModel Map(this CatalogueWebhookModel catalogueWebhookModel)
    {
        return new CatalogueModel
        {
            Uid = catalogueWebhookModel.CatalogueUid,
            Alias = catalogueWebhookModel.CatalogueAlias
        };
    }

    /// <summary>
    ///     Maps a Struct CatalogueModel to a Commercetools CategoryDraft
    /// </summary>
    /// <param name="catalogueModel"></param>
    public static ICategoryDraft MapDraft(this CatalogueModel catalogueModel)
    {

        var localizedString = new LocalizedString { { "en", catalogueModel.Alias } };
        var slug = new LocalizedString { { "en", catalogueModel.Alias.RemoveSpaces() } };
        return new CategoryDraft
        {
            Key = catalogueModel.Uid.ToCommerceKey(),
            Name = localizedString,
            Slug = slug
        };
    }

    /// <summary>
    ///     Maps a Struct CatalogueModel to a Commercetools Category
    /// </summary>
    /// <param name="catalogueModel"></param>
    public static ICategory Map(this CatalogueModel catalogueModel)
    {
        var mapped = catalogueModel.MapDraft();
        return new Category
        {
            Key = mapped.Key,
            Name = mapped.Name,
            Slug = mapped.Slug
        };
    }

    /// <summary>
    ///     Maps a Struct CategoryModel to a Commercetools CategoryDraft
    /// </summary>
    /// <param name="categoryModel"></param>
    /// <param name="localizedSlug">If not provided the Struct CategoryModel UID will be used for all languages</param>
    public static ICategoryDraft MapDraft(this CategoryModel categoryModel, LocalizedString? localizedSlug = null)
    {
        var key = categoryModel.Id.ToCommerceKey();
        var name = LocalizeMapping.Map(categoryModel.Name);
        var parent = GetParent(categoryModel);
        return new CategoryDraft
        {
            Parent = parent,
            Key = key,
            Name = name,
            Slug = localizedSlug ?? LocalizeHelper.GetSlug(name, key)
        };
    }


    /// <summary>
    ///     Maps a Struct CategoryModel to a Commercetools Category
    /// </summary>
    /// <param name="categoryModel"></param>
    /// <param name="localizedSlug">If not provided the Struct CategoryModel UID will be used for all languages</param>
    public static ICategory Map(this CategoryModel categoryModel, LocalizedString? localizedSlug = null)
    {
        var model = MapDraft(categoryModel, localizedSlug);
        var category = new Category
        {
            Parent = new CategoryReference { Id = model.Parent?.Id },
            Key = model.Key,
            Name = model.Name,
            Slug = model.Slug
        };
        return category;
    }


    private static CategoryResourceIdentifier? GetParent(CategoryModel? categoryModel)
    {
        if (categoryModel?.ParentId == null && categoryModel?.CatalogueUid == Guid.Empty)
        {
            return null;
        }

        var parent = categoryModel?.ParentId.ToCommerceKey() ?? categoryModel?.CatalogueUid.ToCommerceKey();

        return new CategoryResourceIdentifier { Key = parent ?? "" };
    }
}