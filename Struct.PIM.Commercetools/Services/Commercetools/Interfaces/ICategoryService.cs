using commercetools.Sdk.Api.Models.Categories;
using Struct.PIM.Api.Models.Catalogue;

namespace Struct.PIM.Commercetools.Services.Commercetools.Interfaces;

public interface ICategoryService
{
    Task<ICategory?> Create(CatalogueModel catalogueModel);
    Task Create(List<CatalogueModel> catalogueModels);
    Task<ICategory?> Create(CategoryModel categoryModel);
    Task Create(List<CategoryModel> categoryModels);
    Task<ICategory?> Update(CatalogueModel catalogueModel);
    Task Update(List<CatalogueModel> categoryModels);
    Task<ICategory?> Update(CategoryModel categoryModel);
    Task Update(List<CategoryModel> categoryModels);
    Task Delete(List<CategoryModel> categoryModels);
    Task Delete(List<Guid> id);
    Task<ICategory?> Delete(Guid id);
    Task<ICategory?> Delete(int id);
    Task Delete(List<int> ids);
}