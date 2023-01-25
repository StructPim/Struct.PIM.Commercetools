using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Struct.PIM.Api.Client.Endpoints.Interfaces;
using Struct.PIM.Api.Models.Product;
using Struct.PIM.Commercetools.Services;
using Struct.PIM.Commercetools.Services.Commercetools.Interfaces;
using Struct.PIM.Commercetools.Webhook.EventKeys;
using Struct.PIM.WebhookModels;
using Swashbuckle.AspNetCore.Annotations;

namespace Struct.PIM.Commercetools.Controllers;

[SwaggerTag("The Product controller handles Product changes from Struct PIM to Commercetools Products")]
[ApiController]
[Route("product")]
public class ProductController : ControllerBase
{
    private readonly IErrorService _errorService;
    private readonly ILogger<ProductController> _logger;
    private readonly IProductEndpoint _productEndpoint;
    private readonly IProductVariantService _productVariantService;

    public ProductController(ILogger<ProductController> logger, IProductEndpoint productEndpoint, IProductVariantService productVariantService, IErrorService errorService)
    {
        _productEndpoint = productEndpoint ?? throw new ArgumentNullException(nameof(productEndpoint));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _productVariantService = productVariantService ?? throw new ArgumentNullException(nameof(productVariantService));
        _errorService = errorService ?? throw new ArgumentNullException(nameof(errorService));
    }

    [SwaggerOperation(
        Summary = "Executes a webhook",
        Description = "Executes a webhook",
        OperationId = "Webhook")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [Produces("application/json")]
    [HttpPost("webhook")]
    public async Task<ActionResult> Webhook([FromBody] ProductWebhookModel? model)
    {
        if (!Request.Headers.TryGetValue("X-Event-Key", out var key))
        {
            _logger.LogWarning("X-Event-Key is missing in webhook when trying to handle product {Model}", model?.ProductIds?.Select(p => p));
            return BadRequest("X-Event-Key is missing");
        }

        if (model == null)
        {
            return BadRequest("No model provided");
        }

        return await HandleWebhook(model, key);


    }

    private async Task<ActionResult> HandleWebhook(ProductWebhookModel model, StringValues key)
    {

        var webhookEventKey = key.ToString();
        _logger.LogInformation("Handle webhook {Event}", webhookEventKey);
        if (model.ProductIds == null || !model.ProductIds.Any())
        {
            return BadRequest("No product ids provided");
        }


        var products = (await _productEndpoint.GetProductsAsync(model.ProductIds.ToList()))?.ToList();
        if (products?.Any() != null)
        {
            switch (webhookEventKey)
            {
                case ProductWebhookEventKeys.Created:
                    await _productVariantService.CreateProducts(products);
                    return !_errorService.HasErrors() ? Ok() : BadRequest(_errorService.GetErrors());
                case ProductWebhookEventKeys.Updated:
                    await _productVariantService.UpdateProducts(products);
                    return !_errorService.HasErrors() ? Ok() : BadRequest(_errorService.GetErrors());
                case ProductWebhookEventKeys.Deleted:
                    await _productVariantService.DeleteProducts(products);
                    return !_errorService.HasErrors() ? Ok() : BadRequest(_errorService.GetErrors());
                default:
                    _logger.LogWarning("No handler for webhook {Event}", webhookEventKey);
                    return BadRequest($"No handler for webhook {webhookEventKey}");
            }
        }

        _logger.LogInformation("No matching Struct products found for {Ids}", model.ProductIds.Select(x => x));
        return BadRequest($"No matching Struct products found for {model.ProductIds.Select(x => x)}");
    }


    [SwaggerOperation(
        Summary = "Get products",
        Description = "Get products",
        OperationId = "Get")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductsResultSet))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [Produces("application/json")]
    [HttpGet]
    public async Task<ActionResult<ProductsResultSet?>> Get(int limit)
    {
        try
        {
            return Ok(await _productEndpoint.GetProductAsync(limit));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [SwaggerOperation(
        Summary = "Get product classifications",
        Description = "Get product classifications",
        OperationId = "Get")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [Produces("application/json")]
    [HttpGet("{productId}/classifications")]
    public async Task<ActionResult<IEnumerable<ProductClassificationModel>?>> GetClassifications(int productId)
    {
        try
        {
            return Ok(await _productEndpoint.GetProductClassificationsAsync(productId));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [SwaggerOperation(
        Summary = "Get variant ids",
        Description = "Get variant ids",
        OperationId = "Get")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [Produces("application/json")]
    [HttpGet("{productId}/variants")]
    public async Task<ActionResult<IEnumerable<int>?>> GetVariants(int productId)
    {
        try
        {
            return Ok(await _productEndpoint.GetVariantIdsAsync(productId));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}