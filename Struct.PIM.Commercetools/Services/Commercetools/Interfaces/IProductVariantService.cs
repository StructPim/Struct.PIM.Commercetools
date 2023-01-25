using commercetools.Sdk.Api.Models.Products;
using Struct.PIM.Api.Models.Product;
using Struct.PIM.Api.Models.Variant;

namespace Struct.PIM.Commercetools.Services.Commercetools.Interfaces;

public interface IProductVariantService
{
    Task CreateProducts(List<ProductModel> products);
    Task<List<IProduct?>> DeleteProducts(List<ProductModel> products);
    Task DeleteProducts(List<int> productIds);
    Task CreateVariants(List<VariantModel> variants);
    Task UpdateProducts(List<ProductModel> products);
    Task DeleteVariants(List<int> variantIds);
    Task DeleteVariants(List<VariantModel> variants);
    Task UpdateVariants(List<VariantModel> variants);
}