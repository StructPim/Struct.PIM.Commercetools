using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Struct.PIM.Api.Client.Endpoints.Interfaces;
using Struct.PIM.Api.Models.Variant;
using Struct.PIM.Commercetools.Services;
using Struct.PIM.Commercetools.Services.Commercetools.Interfaces;
using Struct.PIM.Commercetools.Webhook.EventKeys;
using Struct.PIM.WebhookModels;
using Swashbuckle.AspNetCore.Annotations;

namespace Struct.PIM.Commercetools.Controllers;

[SwaggerTag("The Variant controller handles Variant changes from Struct PIM to Commercetools Variants")]
[ApiController]
[Route("variant")]
public class VariantController : ControllerBase
{
    private readonly IErrorService _errorService;
    private readonly ILogger<VariantController> _logger;
    private readonly IProductVariantService _productVariantService;
    private readonly IVariantEndpoint _variantEndpoint;

    public VariantController(ILogger<VariantController> logger, IVariantEndpoint variantEndpoint, IProductVariantService productVariantService, IErrorService errorService)
    {
        _variantEndpoint = variantEndpoint ?? throw new ArgumentNullException(nameof(variantEndpoint));
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
    public async Task<ActionResult> Webhook([FromBody] VariantWebhookModel? model)
    {
        if (!Request.Headers.TryGetValue("X-Event-Key", out var key))
        {
            _logger.LogWarning("X-Event-Key is missing in webhook when trying to handle variant {Model}", model?.VariantIds?.Select(p => p));
            return BadRequest("X-Event-Key is missing");
        }

        if (model == null)
        {
            return BadRequest("No model provided");
        }

        return await HandleWebhook(model, key);
    }

    private async Task<ActionResult> HandleWebhook(VariantWebhookModel model, StringValues key)
    {

        var webhookEventKey = key.ToString();
        _logger.LogInformation("Handle webhook {Event}", webhookEventKey);
        if (model.VariantIds == null || !model.VariantIds.Any())
        {
            return BadRequest("No variant ids provided");
        }

        var variants = (await _variantEndpoint.GetVariantsAsync(model.VariantIds.ToList()))?.ToList();
        if (variants?.Any() != null)
        {
            switch (webhookEventKey)
            {
                case VariantWebhookEventKeys.Created:
                    await _productVariantService.CreateVariants(variants);
                    return !_errorService.HasErrors() ? Ok() : BadRequest(_errorService.GetErrors());
                case VariantWebhookEventKeys.Updated:
                    await _productVariantService.UpdateVariants(variants);
                    return !_errorService.HasErrors() ? Ok() : BadRequest(_errorService.GetErrors());
                case VariantWebhookEventKeys.Deleted:
                    await _productVariantService.DeleteVariants(variants);
                    return !_errorService.HasErrors() ? Ok() : BadRequest(_errorService.GetErrors());
                default:
                    _logger.LogWarning("No handler for webhook {Event}", webhookEventKey);
                    return BadRequest($"No handler for webhook {webhookEventKey}");
            }
        }

        _logger.LogInformation("No matching Struct variants found for {Ids}", model.VariantIds.Select(x => x));
        return BadRequest($"No matching Struct variants found for {model.VariantIds.Select(x => x)}");
    }

    [SwaggerOperation(
        Summary = "Get variant attributes",
        Description = "Get variant attributes",
        OperationId = "Post")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VariantsResultSet))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [Produces("application/json")]
    [HttpPost]
    public async Task<ActionResult<VariantsResultSet?>> Get([FromBody] VariantRequestModel model)
    {
        if (model.Ids == null)
        {
            return BadRequest("No Ids provided");
        }

        try
        {
            return Ok(await _variantEndpoint.GetVariantsAsync(model.Ids));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    public class VariantRequestModel
    {
        public List<int>? Ids { get; set; }
    }
}