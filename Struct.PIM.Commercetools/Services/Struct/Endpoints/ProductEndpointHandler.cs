using Struct.PIM.Api.Models.Product;

namespace Struct.PIM.Commercetools.Services.Struct.Endpoints;

public interface IProductEndpointHandler
{
    /// <summary>
    ///     Get attribute values
    /// </summary>
    /// <param name="productIds"></param>
    Task<IEnumerable<ProductAttributeValuesModel>?> GetAttributeValues(IEnumerable<int> productIds);

    Task<ProductAttributeValuesModel?> GetAttributeValue(int variantId);
}

public class ProductEndpointHandler : IProductEndpointHandler
{
    private readonly HttpClient _structClient;

    public ProductEndpointHandler(IHttpClientFactory httpClientFactory)
    {
        _structClient = httpClientFactory.CreateClient("StructApiClient");
    }


    /// <summary>
    ///     Get attribute value
    /// </summary>
    /// <param name="variantId"></param>
    public async Task<ProductAttributeValuesModel?> GetAttributeValue(int variantId)
    {
        var response = await _structClient.GetAsync($"/products/{variantId}/attributevalues?globalListValueReferencesOnly=false");
        return await response.Content.ReadFromJsonAsync<ProductAttributeValuesModel>();
    }

    /// <summary>
    ///     Get attribute values
    /// </summary>
    /// <param name="productIds"></param>
    public async Task<IEnumerable<ProductAttributeValuesModel>?> GetAttributeValues(IEnumerable<int> productIds)
    {
        var attributes = new List<ProductAttributeValuesModel>();
        var ids = productIds as List<int> ?? productIds.ToList();

        while (ids.Any())
        {
            var response = await _structClient.PostAsJsonAsync("/products/batch/attributevalues", new ProductValuesRequestModel
            {
                ProductIds = ids.Take(1000).ToList()
            });
            var productAttributeValuesModels = await response.Content.ReadFromJsonAsync<IEnumerable<ProductAttributeValuesModel>>();
            if (productAttributeValuesModels != null)
            {
                attributes.AddRange(productAttributeValuesModels);
            }

            ids.RemoveRange(0, ids.Count < 1000 ? ids.Count : 1000);
        }

        return attributes;
    }
}