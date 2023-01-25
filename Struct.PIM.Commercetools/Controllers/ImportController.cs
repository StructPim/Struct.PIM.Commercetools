using Microsoft.AspNetCore.Mvc;
using Struct.PIM.Api.Client.Endpoints.Interfaces;
using Struct.PIM.Commercetools.Helpers;
using Struct.PIM.Commercetools.Services;
using Struct.PIM.Commercetools.Services.Commercetools.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace Struct.PIM.Commercetools.Controllers;

[SwaggerTag("Import controller")]
[ApiController]
[Route("import")]
public class ImportController : ControllerBase
{
    private readonly ICatalogueEndpoint _catalogueEndpoint;
    private readonly ICategoryService _categoryService;
    private readonly IConfiguration _configuration;
    private readonly IErrorService _errorService;
    private readonly ImportOptions _importOptions;
    private readonly IImportService _importService;
    private readonly ILogger<ImportController> _logger;
    private readonly IProductEndpoint _productEndpoint;
    private readonly IProductStructureEndpoint _productStructureEndpoint;
    private readonly IProductTypeService _productTypeService;
    private readonly IProductVariantService _productVariantService;
    private readonly IProjectSettingsService _projectSettingsService;
    private readonly IVariantEndpoint _variantEndpoint;

    public ImportController(
        ILogger<ImportController> logger,
        IErrorService errorService,
        IConfiguration configuration,
        IProjectSettingsService projectSettingsService,
        ICatalogueEndpoint catalogueEndpoint,
        ICategoryService categoryService,
        IProductStructureEndpoint productStructureEndpoint,
        IProductTypeService productTypeService,
        IProductEndpoint productEndpoint,
        IVariantEndpoint variantEndpoint,
        IProductVariantService productVariantService,
        IImportService importService
    )
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _errorService = errorService ?? throw new ArgumentNullException(nameof(errorService));
        _configuration = configuration;
        _projectSettingsService = projectSettingsService ?? throw new ArgumentNullException(nameof(projectSettingsService));
        _catalogueEndpoint = catalogueEndpoint ?? throw new ArgumentNullException(nameof(catalogueEndpoint));
        _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        _productStructureEndpoint = productStructureEndpoint ?? throw new ArgumentNullException(nameof(productStructureEndpoint));
        _productTypeService = productTypeService ?? throw new ArgumentNullException(nameof(productTypeService));
        _productEndpoint = productEndpoint ?? throw new ArgumentNullException(nameof(productEndpoint));
        _variantEndpoint = variantEndpoint ?? throw new ArgumentNullException(nameof(variantEndpoint));
        _productVariantService = productVariantService ?? throw new ArgumentNullException(nameof(productVariantService));
        _importService = importService ?? throw new ArgumentNullException(nameof(importService));
        _importOptions = _configuration.GetSection("ImportOptions").Get<ImportOptions>();
        _importService.RollBackOnFailure = _importOptions.RollBackOnFailure;
    }

    [SwaggerOperation(
        Summary = "Clean Commerce",
        Description = "Removes Struct PIM catalogues, categories, languages, product structures, products and variants from Commerce Tools")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [Produces("application/json")]
    [HttpGet("clean")]
    public async Task<ActionResult> Clean()
    {
        if (!_importOptions.AllowCleanCommerce)
        {
            return BadRequest("Not allowed");
        }

        var categoryIds = await _catalogueEndpoint.GetCategoryIdsAsync();
        var catalogues = await _catalogueEndpoint.GetCataloguesAsync();
        var productStructures = await _productStructureEndpoint.GetProductStructuresAsync();
        var productIds = await _productEndpoint.GetProductIdsAsync();
        var variantIds = await _variantEndpoint.GetVariantIdsAsync();
        _importService.AddRollBackStep(() => _productVariantService.DeleteVariants(variantIds));
        _importService.AddRollBackStep(() => _productVariantService.DeleteProducts(productIds));
        _importService.AddRollBackStep(() => _productTypeService.Delete(productStructures));
        _importService.AddRollBackStep(() => _categoryService.Delete(categoryIds));
        _importService.AddRollBackStep(() => _categoryService.Delete(catalogues.Select(p => p.Uid).ToList()));

        await _importService.CleanUp();
        return Ok(_errorService.GetErrors());
    }

    [SwaggerOperation(
        Summary = "Initial import PIM",
        Description = "Imports catalogues, categories, languages, product structures, products and variants into Commerce Tools")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [Produces("application/json")]
    [HttpGet("initial")]
    public async Task<ActionResult> Initial()
    {
        try
        {
            // Import Languages
            await _importService.Execute(() => _projectSettingsService.CreateLanguages(),null);

            // Import Catalogues
            var catalogues = await _catalogueEndpoint.GetCataloguesAsync();
            await _importService.Execute(() => _categoryService.Create(catalogues),() => _categoryService.Delete(catalogues.Select(p => p.Uid).ToList()));

            // Import Categories
            var categoryIds = await _catalogueEndpoint.GetCategoryIdsAsync();
            var categories = await _catalogueEndpoint.GetCategoriesAsync(categoryIds);
            await _importService.Execute(() => _categoryService.Create(categories),() => _categoryService.Delete(categoryIds));

            // Import Product structures
            var productStructures = await _productStructureEndpoint.GetProductStructuresAsync();
            await _importService.Execute(() => _productTypeService.Create(productStructures, _configuration.GetSection("IncludeProductStructureAliases").Get<List<string>>()),() => _productTypeService.Delete(productStructures));

            // Import Products
            var productIds = await _productEndpoint.GetProductIdsAsync();
            var productBatchIds = productIds.Batch(1000);
            foreach (var ids in productBatchIds)
            {
                var products = (await _productEndpoint.GetProductsAsync(ids.ToList()))?.ToList();
                if (products != null)
                {
                    await _importService.Execute(() => _productVariantService.CreateProducts(products),() => _productVariantService.DeleteProducts(productIds));

                }
            }

            // Import Variants
            var variantIds = await _variantEndpoint.GetVariantIdsAsync();
            var variantBatchIds = variantIds.Batch(1000);
            foreach (var ids in variantBatchIds)
            {
                var variants = (await _variantEndpoint.GetVariantsAsync(ids.ToList()))?.ToList();
                if (variants != null)
                {
                    await _importService.Execute(() => _productVariantService.CreateVariants(variants),() => _productVariantService.DeleteVariants(variantIds));

                }
            }
        }
        catch (Exception e)
        {
            return BadRequest("Import failed: " + e.Message);
        }

        return Ok("Import success");
    }
}