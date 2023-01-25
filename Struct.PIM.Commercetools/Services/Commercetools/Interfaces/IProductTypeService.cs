using commercetools.Sdk.Api.Models.ProductTypes;
using Struct.PIM.Api.Models.ProductStructure;

namespace Struct.PIM.Commercetools.Services.Commercetools.Interfaces;

public interface IProductTypeService
{
    Task<IProductType?> Create(ProductStructure productStructure, List<string>? includeAliases);
    Task Create(List<ProductStructure> productStructure, List<string>? includeAliases);
    Task Delete(List<ProductStructure> productStructure);
    Task<IProductType?> Delete(Guid id);
    Task<IProductType?> GetByKey(string key);
}