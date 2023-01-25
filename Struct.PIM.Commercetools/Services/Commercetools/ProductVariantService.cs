using commercetools.Base.Client;
using commercetools.Base.Client.Error;
using commercetools.Sdk.Api.Client.RequestBuilders.Projects;
using commercetools.Sdk.Api.Extensions;
using commercetools.Sdk.Api.Models.Categories;
using commercetools.Sdk.Api.Models.Products;
using commercetools.Sdk.Api.Models.ProductTypes;
using Struct.PIM.Api.Client.Endpoints.Interfaces;
using Struct.PIM.Api.Models.Product;
using Struct.PIM.Api.Models.Variant;
using Struct.PIM.Commercetools.Helpers;
using Struct.PIM.Commercetools.Mapping;
using Struct.PIM.Commercetools.Services.Commercetools.Interfaces;
using Struct.PIM.Commercetools.Services.Struct.Endpoints;
using Attribute = Struct.PIM.Api.Models.Attribute.Attribute;

namespace Struct.PIM.Commercetools.Services.Commercetools;

public class ProductVariantService : IProductVariantService
{
    private readonly IAttributeEndpoint _attributeEndpoint;
    private readonly ByProjectKeyRequestBuilder _builder;
    private readonly IConfiguration _configuration;
    private readonly IErrorService _errorService;
    private readonly ILogger _logger;
    private readonly IProductEndpoint _productEndpoint;
    private readonly IProductEndpointHandler _productEndpointHandler;
    private readonly IProductStructureEndpoint _productStructureEndpoint;
    private readonly IProductTypeService _productTypeService;
    private readonly IVariantEndpoint _variantEndpoint;
    private readonly IVariantEndpointHandler _variantEndpointHandler;

    public ProductVariantService(ILogger<ProductVariantService> logger, IClient client, IProductEndpoint productEndpoint, IVariantEndpoint variantEndpoint, IProductTypeService productTypeService, IProductStructureEndpoint productStructureEndpoint, IAttributeEndpoint attributeEndpoint, IConfiguration configuration, IErrorService errorService, IProductEndpointHandler productEndpointHandler, IVariantEndpointHandler variantEndpointHandler)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _builder = client != null
            ? client.WithApi().WithProjectKey(Settings.CommerceProjectKey)
            : throw new ArgumentNullException(nameof(client));
        _productEndpoint = productEndpoint ?? throw new ArgumentNullException(nameof(productEndpoint));
        _variantEndpoint = variantEndpoint ?? throw new ArgumentNullException(nameof(variantEndpoint));
        _productTypeService = productTypeService ?? throw new ArgumentNullException(nameof(productTypeService));
        _productStructureEndpoint = productStructureEndpoint ?? throw new ArgumentNullException(nameof(productStructureEndpoint));
        _attributeEndpoint = attributeEndpoint ?? throw new ArgumentNullException(nameof(attributeEndpoint));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _errorService = errorService ?? throw new ArgumentNullException(nameof(errorService));
        _productEndpointHandler = productEndpointHandler ?? throw new ArgumentNullException(nameof(productEndpointHandler));
        _variantEndpointHandler = variantEndpointHandler ?? throw new ArgumentNullException(nameof(variantEndpointHandler));

    }

    /// <summary>
    ///     Creates products in commercetools
    /// </summary>
    public async Task CreateProducts(List<ProductModel> products)
    {
        foreach (var product in products)
        {
            await CreateProduct(product);
        }

    }

    public async Task UpdateProducts(List<ProductModel> products)
    {
        foreach (var product in products)
        {
            var productId = product.Id.ToCommerceKey();
            var classifications = (await _productEndpoint.GetProductClassificationsAsync(product.Id))?.ToList();
            if (classifications == null)
            {
                var err = $"Failed to update the product with with {productId} in Commercetools. No classifications defined";
                _logger.LogError("{Err}", err);
                _errorService.AddError(err);
                continue;
            }

            var existing = await GetByKey(product.Id.ToCommerceKey(), "productType", "masterData.current.categories[*]");
            if (existing == null)
            {
                var err = $"Failed to update the product with with {productId} in Commercetools. Product not found in Commerce Tools";
                _logger.LogError("{Err}", err);
                _errorService.AddError(err);
                continue;
            }

            var productType = await _productTypeService.GetByKey(product.ProductStructureUid.ToCommerceKey());
            if (productType == null)
            {
                var err = $"Failed to update the product with with {productId} in Commercetools. No product type found";
                _logger.LogError("{Err}", err);
                _errorService.AddError(err);
                continue;
            }

            if (productType.Key.ToCommerceKey() != existing.ProductType.Obj.Key)
            {
                // Product type has been altered
                // Commercetools is not supporting this scenario, but
                var recreateProductsOnProductStructureChange = _configuration.GetSection("RecreateProductsOnProductStructureChange").Get<bool>();
                if (recreateProductsOnProductStructureChange)
                {
                    _logger.LogWarning("RecreateProductsOnProductStructureChange for product, {Key}", productId);
                    // RecreateProductsOnProductStructureChange has been set to true
                    await RecreateProduct(product);
                    continue;
                }
            }

            var classification = classifications.HandleClassification();

            var pimMappedProduct = product.Map(classification.Item1, classification.Item2);


            var update = new ProductUpdate();

            await AssignProductActions(update, product.Id, productType, pimMappedProduct, existing);

            if (!update.Actions.Any())
            {
                _logger.LogInformation("No updates for product, {Key}", productId);
                continue;
            }

            await UpdateProduct(productId, update, existing.Version);
        }
    }

    public async Task DeleteVariants(List<int> variantIds)
    {
        var tasks = variantIds.Select(p => _builder.Products().WithKey(p.ToCommerceKey()).Delete().ExecuteAsync()).ToList();
        while (tasks.Any())
        {
            var finishedTask = await Task.WhenAny(tasks);
            tasks.Remove(finishedTask);
        }
    }

    public async Task DeleteVariants(List<VariantModel> variants)
    {
        foreach (var variant in variants)
        {
            var existingProduct = await GetByKey(variant.ProductId.ToCommerceKey());
            if (existingProduct == null)
            {
                continue;
            }

            var variantValues = (await _variantEndpoint.GetVariantAttributeValuesAsync(variant.Id))?.Values.ToDictionary(x => x.Key, x => x.Value);

            if (variantValues == null)
            {
                continue;
            }

            var actions = variantValues.Where(p => p.Key.ToLower() == "sku").Select(sku => new ProductRemoveVariantAction
            {
                Sku = sku.Value.ToString(),
                Staged = false
            }).ToList();
            var productUpdateActions = new List<IProductUpdateAction>();
            productUpdateActions.AddRange(actions);

            try
            {

                var productUpdate = new ProductUpdate
                {
                    Version = existingProduct.Version,
                    Actions = productUpdateActions
                };
                var ctProduct = await _builder.Products().WithKey(variant.ProductId.ToCommerceKey()).Post(productUpdate).ExecuteAsync();
                _logger.LogInformation("Update product variant with uid {Id}", ctProduct.Id);
            }
            catch (BadRequestException e)
            {
                var err = $"Failed to remove the variant {variant.Id.ToCommerceKey()}, in Commercetools.";
                _logger.LogError("{Err} The error was {Error}", err, e.ResolveMessage());
                _errorService.AddError(err);
            }
            catch (NotFoundException)
            {
                var err = $"Failed to remove the variant, {variant.Id.ToCommerceKey()}, in Commercetools, since it does not exist";
                _logger.LogError("{Err}", err);
                _errorService.AddError(err);
            }
        }
    }

    public async Task UpdateVariants(List<VariantModel> variants)
    {
        var attributes = (await _attributeEndpoint.GetAttributesAsync())?.ToList();
        if (attributes != null)
        {
            foreach (var variant in variants)
            {
                await UpdateVariant(variant, attributes);
            }
        }
    }

    /// <summary>
    ///     Deletes products in commercetools
    /// </summary>
    public async Task DeleteProducts(List<int> productIds)
    {
        foreach (var productId in productIds)
        {
            await DeleteProduct(productId);
        }
    }

    /// <summary>
    ///     Deletes products in commercetools
    /// </summary>
    public async Task<List<IProduct?>> DeleteProducts(List<ProductModel> products)
    {
        var tasks = products.Select(async model => await DeleteProduct(model));
        return (await Task.WhenAll(tasks)).ToList();
    }

    public async Task CreateVariants(List<VariantModel> variants)
    {
        var attributes = (await _attributeEndpoint.GetAttributesAsync())?.ToList();
        if (attributes != null)
        {
            foreach (var variant in variants)
            {
                await CreateVariant(variant, attributes);
            }
        }
    }


    /// <summary>
    ///     Creates a product in commercetools
    /// </summary>
    public async Task CreateProduct(ProductModel product)
    {
        var classifications = (await _productEndpoint.GetProductClassificationsAsync(product.Id))?.ToList();
        var productId = product.Id.ToCommerceKey();
        if (classifications == null)
        {
            var err = $"Failed to create the product, {productId} in Commercetools. No classifications defined.";
            _logger.LogError("{Err}", err);
            _errorService.AddError(err);
            return;
        }

        var classification = classifications.HandleClassification();


        var existing = await GetByKey(productId);
        if (existing == null)
        {

            var result = await CreateProduct(product.MapDraft(classification.Item1, classification.Item2));

            if (result != null)
            {
                var update = new ProductUpdate();
                await AssignProductAttributeValueActions(update, product);

                if (update.Actions.Any())
                {
                    await UpdateProduct(productId, update);
                }
            }
        }
        else
        {
            _logger.LogInformation("Skip create product with {Id}, since it already exist", product.Id);
        }

    }

    private async Task AssignProductActions(IProductUpdate productUpdate, int pimProductId, IProductType productType, IProduct pimMappedProduct, IProduct? commerceToolsProduct)
    {
        await AssignProductAttributeValueActions(productUpdate, productType, pimProductId);
        if (commerceToolsProduct != null)
        {
            AssignProductCategoryActions(productUpdate, pimMappedProduct, commerceToolsProduct);
        }
    }

    private void AssignProductCategoryActions(IProductUpdate productUpdate, IProduct pimMappedProduct, IProduct commerceToolsProduct)
    {
        productUpdate.Actions ??= new List<IProductUpdateAction>();
        var missing = pimMappedProduct.MasterData.Current.Categories.ExceptBy(commerceToolsProduct.MasterData.Current.Categories.Select(p => p.Obj.Key), x => x.Obj.Key);
        productUpdate.Actions.AddRange(missing.Select(q => new ProductAddToCategoryAction
            { Staged = false, Category = new CategoryResourceIdentifier { Key = q.Obj.Key } }));
        var deleted = commerceToolsProduct.MasterData.Current.Categories.ExceptBy(pimMappedProduct.MasterData.Current.Categories.Select(p => p.Obj.Key), x => x.Obj.Key);
        productUpdate.Actions.AddRange(deleted.Select(q => new ProductRemoveFromCategoryAction
            { Staged = false, Category = new CategoryResourceIdentifier { Key = q.Obj.Key } }));
    }

    private async Task AssignProductAttributeValueActions(IProductUpdate productUpdate, IProductType productType, int pimProductId)
    {
        productUpdate.Actions ??= new List<IProductUpdateAction>();
        var productAttributeValues = await _productEndpointHandler.GetAttributeValue(pimProductId);
        if (productAttributeValues != null)
        {
            productUpdate.Actions.AddRange(productAttributeValues.Values
                .Where(attribute => productType.Attributes.Any(r => r.Name == attribute.Key && r.Name.ToLower() != "sku"))
                .Select(q => new ProductSetAttributeInAllVariantsAction
                {
                    Name = q.Key,
                    Value = AttributeHelper.ResolveValue(_builder, q.Value, pimProductId + "_" + q.Key),
                    Staged = false
                }).Where(p => p.Value != null));
        }
    }

    private async Task AssignProductAttributeValueActions(IProductUpdate productUpdate, ProductModel pimProduct)
    {
        productUpdate.Actions ??= new List<IProductUpdateAction>();
        var productType = await _productTypeService.GetByKey(pimProduct.ProductStructureUid.ToCommerceKey());
        if (productType != null)
        {
            await AssignProductAttributeValueActions(productUpdate, productType, pimProduct.Id);
        }
    }

    private async Task UpdateVariant(VariantModel variant, List<Attribute> attributes)
    {
        var existingProduct = await GetByKey(variant.ProductId.ToCommerceKey(), "masterData.current.variants[*]");

        if (existingProduct == null)
        {
            return;
        }

        if (!ExistingVariant(variant, existingProduct))
        {
            await CreateVariant(variant, attributes);
            return;
        }

        var allowedVariantAttribute =
            await DetermineAllowedVariantAttributes(attributes, variant);

        if (allowedVariantAttribute == null)
        {
            return;
        }


        if (allowedVariantAttribute.VariantValues != null)
        {
            foreach (var item in allowedVariantAttribute.VariantValues)
            {

                var productUpdate = new ProductUpdate
                {
                    Version = existingProduct.Version,
                    Actions = new List<IProductUpdateAction>
                    {
                        new ProductSetAttributeAction
                        {

                            Sku = allowedVariantAttribute.Sku,
                            Staged = false,
                            Name = item.Key,
                            Value = item.Value
                        }
                    }
                };
                try
                {
                    var ctProduct = await _builder.Products().WithKey(variant.ProductId.ToCommerceKey()).Post(productUpdate).ExecuteAsync();
                    _logger.LogInformation("Update product variant with uid {Id}", ctProduct.Id);
                }
                catch (BadRequestException e)
                {
                    var err = $"Failed to update the variant, {variant.Id.ToCommerceKey()}, in Commercetools";
                    _logger.LogError("{Err} The error was {Message}", err, e.ResolveMessage());
                    _errorService.AddError(err);
                }
                catch (NotFoundException)
                {
                    var err = $"Failed to update the product variant, {variant.Id.ToCommerceKey()}, in Commercetools, since it does not exist";
                    _logger.LogWarning("{Err}", err);
                    _errorService.AddError(err);
                }
            }
        }
    }

    private async Task RecreateProduct(ProductModel product)
    {
        // Delete the product
        await DeleteProduct(product);
        // Create product
        await CreateProduct(product);
        // Create variant(s)
        var variantIds = await _productEndpoint.GetVariantIdsAsync(product.Id);
        var variants = await _variantEndpoint.GetVariantsAsync(variantIds);
        await CreateVariants(variants.ToList());
    }

    private async Task UpdateProduct(string productId, ProductUpdate productUpdate, long version = 1)
    {
        productUpdate.Version = version;

        try
        {
            var ctProduct = await _builder.Products().WithKey(productId).Post(productUpdate).ExecuteAsync();
            _logger.LogInformation("Update product attributes with uid {Id}", ctProduct.Id);
        }
        catch (BadRequestException e)
        {
            var err = $"Failed to update the product, {productId}, in Commercetools.";
            _logger.LogError("{Err}. The error was {Message}", err, e.ResolveMessage());
            _errorService.AddError(err);
        }
        catch (NotFoundException)
        {
            var err = $"Failed to update the product {productId}, since it does not exist in Commercetools";
            _logger.LogError("{Err}", err);
            _errorService.AddError(err);
        }
    }

    /// <summary>
    ///     Deletes a product in commercetools
    /// </summary>
    public async Task<IProduct?> DeleteProduct(int productId)
    {
        var key = productId.ToCommerceKey();
        var existing = await GetByKey(key);
        if (existing == null)
        {
            var err = $"Could not delete the product {key}, in Commercetools since it does not exist";
            _logger.LogError("{Err}", err);
            _errorService.AddError(err);
            return null;
        }

        try
        {
            var unpublished = await UnPublish(existing);
            if (unpublished == null)
            {
                return null;
            }

            var deletedProduct = await _builder.Products().WithKey(key).Delete().WithVersion(unpublished.Version).ExecuteAsync();
            _logger.LogInformation("Delete product {Key}", key);
            return deletedProduct;

        }
        catch (BadRequestException e)
        {
            var err = $"Failed to delete the product, {key}, in Commercetools";
            _logger.LogError("{Err} The error was {Error}", err, e.ResolveMessage());
            _errorService.AddError(err);
            return null;
        }
        catch (NotFoundException)
        {
            var err = $"Failed to delete the product, {key}, in Commercetools, since it does not exist";
            _logger.LogError("{Err}", err);
            _errorService.AddError(err);
            return null;
        }
    }

    /// <summary>
    ///     Deletes a product in commercetools
    /// </summary>
    public async Task<IProduct?> DeleteProduct(ProductModel product)
    {
        return await DeleteProduct(product.Id);
    }

    private async Task<IProduct?> UnPublish(IProduct model)
    {
        try
        {
            var productUpdate = new ProductUpdate
            {
                Version = model.Version,
                Actions = new List<IProductUpdateAction>
                    { new ProductUnpublishAction() }
            };
            return await _builder.Products().WithKey(model.Key).Post(productUpdate).ExecuteAsync();
        }
        catch (BadRequestException e)
        {
            var err = $"Failed to un publish the product, {model.Key}, in Commercetools";
            _logger.LogError("{Err} The error was {Error}", err, e.ResolveMessage());
            _errorService.AddError(err);
            return null;
        }
        catch (NotFoundException)
        {
            var err = $"Failed to un publish the product, {model.Key}, in Commercetools, since it does not exist";
            _logger.LogError("{Err}", err);
            _errorService.AddError(err);
            return null;
        }
    }

    private async Task<IProduct?> CreateProduct(IProductDraft productDraft)
    {
        try
        {
            var result = await _builder.Products().Post(productDraft).ExecuteAsync();
            _logger.LogInformation("Create product with uid {Key}", productDraft.Key);
            return result;
        }
        catch (Exception e)
        {
            var err = $"Failed to create the product, {productDraft.Key}, in Commercetools";
            _logger.LogError("{Err} The error was {Error}", err, e.ResolveMessage());
            _errorService.AddError(err);
            return null;
        }
    }

    private async Task CreateVariant(VariantModel variant, List<Attribute> attributes)
    {
        var existingProduct = await GetByKey(variant.ProductId.ToCommerceKey(), "masterData.current.variants[*]");
        if (existingProduct == null || ExistingVariant(variant, existingProduct))
        {
            return;
        }

        var allowedVariantAttribute =
            await DetermineAllowedVariantAttributes(attributes, variant);

        if (allowedVariantAttribute == null)
        {
            return;
        }

        try
        {
            if (allowedVariantAttribute.VariantValues != null)
            {
                var productAction = new ProductAddVariantAction
                {
                    Key = variant.Id.ToCommerceKey(),
                    Staged = false,
                    Attributes = GetAttributes(allowedVariantAttribute.VariantValues, existingProduct)
                };
                if (allowedVariantAttribute.Sku != null)
                {
                    productAction.Sku = allowedVariantAttribute.Sku;
                }

                var productUpdate = new ProductUpdate
                {
                    Version = existingProduct.Version,
                    Actions = new List<IProductUpdateAction>
                        { productAction }
                };
                var ctProduct = await _builder.Products().WithKey(variant.ProductId.ToCommerceKey()).Post(productUpdate).ExecuteAsync();
                _logger.LogInformation("Update product variant with uid {Id}", ctProduct.Id);
            }
        }
        catch (BadRequestException e)
        {
            var err = $"Failed to create the product variant, {variant.Id.ToCommerceKey()}, in Commercetools";
            _logger.LogError("{Err} The error was {Error}", err, e.ResolveMessage());
            _errorService.AddError(err);
        }
        catch (NotFoundException)
        {
            var err = $"Failed to create the product variant, {variant.Id.ToCommerceKey()}, in Commercetools, since it does not exist";
            _logger.LogError("{Err}", err);
            _errorService.AddError(err);
        }
    }

    private static List<IAttribute> GetAttributes(Dictionary<string, object> allowedVariantAttributes, IProduct existingProduct)
    {

        return allowedVariantAttributes.Concat(existingProduct.MasterData.Current.MasterVariant.Attributes.ToDictionary(x => x.Name, x => x.Value).Where(p => !allowedVariantAttributes.ContainsKey(p.Key))).Select(p => new commercetools.Sdk.Api.Models.Products.Attribute
        {
            Name = p.Key,
            Value = p.Value
        }).ToList<IAttribute>();
    }

    private async Task<AllowedVariantAttribute?> DetermineAllowedVariantAttributes(List<Attribute> attributes, VariantModel variant)
    {

        var productType = await _productTypeService.GetByKey(variant.ProductStructureUid.ToCommerceKey());
        var productStructure = await _productStructureEndpoint.GetProductStructureAsync(variant.ProductStructureUid);
        var variantValues = (await _variantEndpointHandler.GetAttributeValue(variant.Id))?.Values.ToDictionary(x => x.Key, x => x.Value);

        if (productType == null || productStructure == null || variantValues == null)
        {
            return null;
        }

        var allowedVariantAttribute = new AllowedVariantAttribute();

        var sku = variantValues.FirstOrDefault(p => p.Key.ToLower() == "sku");
        allowedVariantAttribute.Sku = sku.Value.ToString();
        variantValues.ToList().Remove(sku);
        allowedVariantAttribute.VariantValues = variantValues
            .Where(p =>
                productType.Attributes.Any(r =>
                {
                    var attribute = attributes.FirstOrDefault(q => q.Alias == r.Name);
                    if (attribute != null)
                    {
                        return r.Name == p.Key &&
                               !productStructure.VariantAttributeInherits(attribute.Uid);
                    }

                    return false;
                })
            )
            .Select(q => new commercetools.Sdk.Api.Models.Products.Attribute
            {
                Name = q.Key,
                Value = AttributeHelper.ResolveValue(_builder, q.Value, variant.Id + "_" + q.Key)
            })
            .Where(p => p.Value != null)
            .ToDictionary(x => x.Name, x => x.Value);

        return allowedVariantAttribute;
    }

    /// <summary>
    ///     Returns true if a PIM variant exists for the given Commertools product
    /// </summary>
    /// <param name="variant"></param>
    /// ///
    /// <param name="product"></param>
    public bool ExistingVariant(VariantModel variant, IProduct product)
    {
        return product.MasterData.Current.Variants.Any(p => p.Key == variant.Id.ToCommerceKey());
    }

    /// <summary>
    ///     Get a Product by key from Commercetools
    /// </summary>
    /// <param name="key"></param>
    /// <param name="expands"></param>
    public async Task<IProduct?> GetByKey(string key, params string[]? expands)
    {
        try
        {
            var request = _builder.Products().WithKey(key.ToCommerceKey()).Get();
            expands?.ToList().ForEach(expand => request.WithExpand(expand));
            return await request.ExecuteAsync();
        }
        catch (Exception e)
        {
            var err = $"Failed to get the product {key.ToCommerceKey()}, in Commercetools.";
            _logger.LogError("{Err} The error was {Error}", err, e.ResolveMessage());
            return null;
        }
    }

    private class AllowedVariantAttribute
    {
        public string? Sku { get; set; }
        public Dictionary<string, dynamic>? VariantValues { get; set; }
    }
}