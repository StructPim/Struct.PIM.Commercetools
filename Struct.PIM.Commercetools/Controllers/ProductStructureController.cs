using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Struct.PIM.Api.Client.Endpoints.Interfaces;
using Struct.PIM.Api.Models.ProductStructure;
using Struct.PIM.Commercetools.Services;
using Struct.PIM.Commercetools.Services.Commercetools.Interfaces;
using Struct.PIM.Commercetools.Webhook.EventKeys;
using Struct.PIM.WebhookModels;
using Swashbuckle.AspNetCore.Annotations;

namespace Struct.PIM.Commercetools.Controllers;

[SwaggerTag("The ProductStructure controller handles ProductStructure changes from Struct PIM to Commercetools Producttypes")]
[ApiController]
[Route("productstructure")]
public class ProductStructureController : ControllerBase
{
    private readonly IProductTypeService _commerceProductTypeService;
    private readonly IConfiguration _configuration;
    private readonly IErrorService _errorService;
    private readonly ILogger<ProductStructureController> _logger;
    private readonly IProductStructureEndpoint _productStructureEndpoint;

    public ProductStructureController(ILogger<ProductStructureController> logger, IProductStructureEndpoint productStructureEndpoint, IProductTypeService commerceProductTypeService, IConfiguration configuration, IErrorService errorService)
    {
        _productStructureEndpoint = productStructureEndpoint ?? throw new ArgumentNullException(nameof(productStructureEndpoint));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _commerceProductTypeService = commerceProductTypeService ?? throw new ArgumentNullException(nameof(commerceProductTypeService));
        _configuration = configuration;
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
    public async Task<ActionResult> Webhook([FromBody] ProductStructureWebhookModel? model)
    {
        if (!Request.Headers.TryGetValue("X-Event-Key", out var key))
        {
            _logger.LogWarning("X-Event-Key is missing in webhook when trying to handle product structure {Model}", model?.ProductStructureAlias);
            return BadRequest("X-Event-Key is missing");
        }

        if (model == null)
        {
            return BadRequest("No model provided");
        }

        return await HandleWebhook(model, key);


    }

    private async Task<ActionResult> HandleWebhook(ProductStructureWebhookModel model, StringValues key)
    {

        var webhookEventKey = key.ToString();
        _logger.LogInformation("Handle webhook {Event}", webhookEventKey);
        if (model.ProductStructureUid == new Guid())
        {
            return BadRequest("No ProductStructureUid provided");
        }

        var productStructure = await _productStructureEndpoint.GetProductStructureAsync(model.ProductStructureUid);
        if (productStructure == null)
        {
            _logger.LogInformation("No matching Struct product structure found for {Uid}", model.ProductStructureUid);
            return BadRequest($"No matching Struct categories found for {model.ProductStructureUid}");
        }

        switch (webhookEventKey)
        {
            case ProductStructureWebhookEventKeys.Created:
                await _commerceProductTypeService.Create(productStructure, _configuration.GetSection("IncludeProductStructureAliases").Get<List<string>>());
                return !_errorService.HasErrors() ? Ok() : BadRequest(_errorService.GetErrors());
            case ProductStructureWebhookEventKeys.Updated:
                return BadRequest("Updating a product type is not supported by Commercetools");
            case ProductStructureWebhookEventKeys.Deleted:
                await _commerceProductTypeService.Delete(model.ProductStructureUid);
                return !_errorService.HasErrors() ? Ok() : BadRequest(_errorService.GetErrors());
            default:
                _logger.LogWarning("No handler for webhook {Event}", webhookEventKey);
                return BadRequest($"No handler for webhook {webhookEventKey}");
        }
    }


    [SwaggerOperation(
        Summary = "Get product structure",
        Description = "Get product structure",
        OperationId = "Get")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductStructure))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [Produces("application/json")]
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductStructure>?> Get(Guid id)
    {
        try
        {
            return Ok(await _productStructureEndpoint.GetProductStructureAsync(id));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}