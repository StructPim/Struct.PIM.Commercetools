using commercetools.Base.Client;
using commercetools.Sdk.Api.Client.RequestBuilders.Projects;
using commercetools.Sdk.Api.Extensions;
using commercetools.Sdk.Api.Models.ProductTypes;
using Struct.PIM.Api.Client.Endpoints.Interfaces;
using Struct.PIM.Api.Models.ProductStructure;
using Struct.PIM.Commercetools.Helpers;
using Struct.PIM.Commercetools.Mapping;
using Struct.PIM.Commercetools.Services.Commercetools.Interfaces;

namespace Struct.PIM.Commercetools.Services.Commercetools;

public class ProductTypeService : IProductTypeService
{
    private readonly ByProjectKeyRequestBuilder _builder;
    private readonly IErrorService _errorService;
    private readonly ILogger<ProductTypeService> _logger;
    private readonly IAttributeEndpoint _structAttributeEndpoint;

    public ProductTypeService(IClient client, ILogger<ProductTypeService> logger, IAttributeEndpoint structAttributeEndpoint, IErrorService errorService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _builder = client != null
            ? client.WithApi().WithProjectKey(Settings.CommerceProjectKey)
            : throw new ArgumentNullException(nameof(client));
        _structAttributeEndpoint = structAttributeEndpoint ?? throw new ArgumentNullException(nameof(structAttributeEndpoint));
        _errorService = errorService ?? throw new ArgumentNullException(nameof(errorService));
    }


    /// <summary>
    ///     Creates a ProductTypes in commercetools
    /// </summary>
    public async Task Create(List<ProductStructure> productStructure, List<string>? includeAliases = null)
    {
        var drafts = productStructure.Select(p => Create(p, includeAliases)).ToList();
        while (drafts.Any())
        {
            var finishedTask = await Task.WhenAny(drafts);
            drafts.Remove(finishedTask);
        }
    }

    public async Task Delete(List<ProductStructure> productStructures)
    {
        foreach (var productStructure in productStructures)
        {
            await Delete(productStructure.Uid);
        }
    }

    /// <summary>
    ///     Creates a ProductType in commercetools
    /// </summary>
    public async Task<IProductType?> Create(ProductStructure productStructure, List<string>? includeAliases = null)
    {
        var productAttributeUids = ProductStructureHelper.GetProductAttributeUids(productStructure);
        var productAttributes = await _structAttributeEndpoint.GetAttributesAsync(productAttributeUids);
        var variantAttributeUids = ProductStructureHelper.GetVariantAttributeUids(productStructure);
        var variantAttributes = await _structAttributeEndpoint.GetAttributesAsync(variantAttributeUids);
        var draft = productStructure.MapDraft(productAttributes, variantAttributes, includeAliases);
        try
        {
            var productType = await _builder.ProductTypes().Post(draft).ExecuteAsync();
            _logger.LogInformation("Create product type with uid {Key}", draft.Key);
            return productType;
        }
        catch (Exception e)
        {
            var err = $"Failed to create the productType with {draft.Key} in Commercetools.";
            _logger.LogError("{Err} The error was {Message}", err, e.ResolveMessage());
            _errorService.AddError(err);
            return null;
        }

    }

    /// <summary>
    ///     Get a ProductType by key from Commercetools
    /// </summary>
    /// <param name="key"></param>
    public async Task<IProductType?> GetByKey(string key)
    {
        try
        {
            return await _builder.ProductTypes().WithKey(key).Get().ExecuteAsync();
        }
        catch (Exception e)
        {
            _logger.LogWarning("Failed to get the product type with {Key}, in Commercetools. The error was {Error}", key, e.ResolveMessage());
            return null;
        }
    }

    /// <summary>
    ///     Deletes a ProductType in commercetools
    /// </summary>
    /// <param name="id">The Struct ProductStructure Guid</param>
    public async Task<IProductType?> Delete(Guid id)
    {
        var key = id.ToCommerceKey();
        var existing = await GetByKey(key);
        if (existing == null)
        {
            _logger.LogWarning("Could not delete the product type {Key}, in Commercetools since it does not exist", key);
            return null;
        }

        try
        {
            var category = await _builder.ProductTypes().WithKey(key).Delete().WithVersion(existing.Version).ExecuteAsync();
            _logger.LogInformation("Delete product type {Key}", key);
            return category;
        }
        catch (Exception e)
        {
            var err = $"Failed to delete the productType with {key} in Commercetools.";
            _logger.LogError("{Err} The error was {Message}", err, e.ResolveMessage());
            _errorService.AddError(err);
            return null;
        }
    }
}