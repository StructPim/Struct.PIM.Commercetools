using commercetools.Sdk.Api.Models.Categories;
using commercetools.Sdk.Api.Models.Common;
using commercetools.Sdk.Api.Models.Products;
using commercetools.Sdk.Api.Models.ProductTypes;
using Struct.PIM.Api.Models.Product;
using Struct.PIM.Commercetools.Helpers;

namespace Struct.PIM.Commercetools.Mapping;

public static class ProductMapping
{
    /// <summary>
    ///     Maps a Struct ProductModel to a Commercetools ProductDraft
    /// </summary>
    /// <param name="productModel"></param>
    /// <param name="primaryCategoryId"></param>
    /// <param name="categoryIds"></param>
    /// <param name="localizedSlug">If not provided the Struct ProductModel UID will be used for all languages</param>
    /// <param name="publish"></param>
    public static IProductDraft MapDraft(
        this ProductModel productModel,
        int? primaryCategoryId,
        IEnumerable<int>? categoryIds,
        LocalizedString? localizedSlug = null,
        bool publish = true)
    {
        var key = productModel.Id.ToCommerceKey();
        var name = LocalizeMapping.Map(productModel.Name);
        var productTypeResourceIdentifier = new ProductTypeResourceIdentifier { Key = productModel.ProductStructureUid.ToCommerceKey() };
        var draft = new ProductDraft
        {
            Key = key,
            Name = name,
            ProductType = productTypeResourceIdentifier,
            Slug = localizedSlug ?? LocalizeHelper.GetSlug(name, key),
            Publish = publish
        };
        if (primaryCategoryId == null)
        {
            return draft;
        }

        var categories = new List<CategoryResourceIdentifier>
        {
            new() { Key = primaryCategoryId.ToCommerceKey() }
        };

        if (categoryIds != null)
        {
            categories.AddRange(categoryIds.Select(c => new CategoryResourceIdentifier
                { Key = c.ToCommerceKey() }));
        }

        draft.Categories = categories.ToList<ICategoryResourceIdentifier>();

        return draft;
    }

    /// <summary>
    ///     Maps a Struct ProductModel to a Commercetools Product
    /// </summary>
    /// <param name="productModel"></param>
    /// <param name="primaryCategoryId"></param>
    /// ///
    /// <param name="categoryIds"></param>
    /// <param name="localizedSlug">If not provided the Struct ProductModel UID will be used for all languages</param>
    public static IProduct Map(
        this ProductModel productModel,
        int? primaryCategoryId,
        IEnumerable<int>? categoryIds,
        LocalizedString? localizedSlug = null)
    {
        var key = productModel.Id.ToCommerceKey();
        var name = LocalizeMapping.Map(productModel.Name);
        var product = new Product
        {
            Key = key,
            MasterData = new ProductCatalogData
            {
                Current = new ProductData
                {
                    Name = name,
                    Slug = localizedSlug ?? LocalizeHelper.GetSlug(name, key)

                }
            }
        };
        if (primaryCategoryId == null)
        {
            return product;
        }

        var categoryReferences = new List<ICategoryReference>
        {
            new CategoryReference
            {
                Obj = new Category
                    { Key = primaryCategoryId.ToCommerceKey() }
            }
        };
        if (categoryIds != null)
        {
            categoryReferences.AddRange(categoryIds.Select(c => new CategoryReference
            {
                Obj = new Category
                    { Key = c.ToCommerceKey() }
            }));
        }

        product.MasterData.Current.Categories = categoryReferences;

        return product;
    }
}