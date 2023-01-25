using Struct.PIM.Api.Models.Variant;

namespace Struct.PIM.Commercetools.Services.Struct.Endpoints;

public interface IVariantEndpointHandler
{
    /// <summary>
    ///     Get attribute values
    /// </summary>
    /// <param name="variantsIds"></param>
    Task<IEnumerable<VariantAttributeValuesModel>?> GetAttributeValues(List<int> variantsIds);

    /// <summary>
    ///     Get attribute value
    /// </summary>
    /// <param name="variantId"></param>
    Task<VariantAttributeValuesModel?> GetAttributeValue(int variantId);
}

public class VariantEndpointHandler : IVariantEndpointHandler
{
    private readonly HttpClient _structClient;

    public VariantEndpointHandler(IHttpClientFactory httpClientFactory)
    {
        _structClient = httpClientFactory.CreateClient("StructApiClient");
    }

    /// <summary>
    ///     Get attribute values
    /// </summary>
    /// <param name="variantsIds"></param>
    public async Task<IEnumerable<VariantAttributeValuesModel>?> GetAttributeValues(List<int> variantsIds)
    {
        var attributes = new List<VariantAttributeValuesModel>();

        while (variantsIds.Any())
        {
            var response = await _structClient.PostAsJsonAsync("/variants/batch/attributevalues", new VariantValuesRequestModel
            {
                VariantIds = variantsIds.Take(1000).ToList()
            });
            var variantAttributeValuesModels = await response.Content.ReadFromJsonAsync<IEnumerable<VariantAttributeValuesModel>>();
            if (variantAttributeValuesModels != null)
            {
                attributes.AddRange(variantAttributeValuesModels);
            }

            variantsIds.RemoveRange(0, variantsIds.Count < 1000 ? variantsIds.Count : 1000);
        }

        return attributes;
    }

    /// <summary>
    ///     Get attribute value
    /// </summary>
    /// <param name="variantId"></param>
    public async Task<VariantAttributeValuesModel?> GetAttributeValue(int variantId)
    {
        var response = await _structClient.GetAsync($"/variants/{variantId}/attributevalues?globalListValueReferencesOnly=false");
        return await response.Content.ReadFromJsonAsync<VariantAttributeValuesModel>();
    }
}