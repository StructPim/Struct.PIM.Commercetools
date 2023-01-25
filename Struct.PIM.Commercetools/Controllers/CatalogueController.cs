using commercetools.Sdk.Api.Models.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Struct.PIM.Api.Models.Catalogue;
using Struct.PIM.Commercetools.Mapping;
using Struct.PIM.Commercetools.Services;
using Struct.PIM.Commercetools.Services.Commercetools.Interfaces;
using Struct.PIM.Commercetools.Webhook.EventKeys;
using Struct.PIM.WebhookModels;
using Swashbuckle.AspNetCore.Annotations;

namespace Struct.PIM.Commercetools.Controllers;

[ApiController]
[Route("catalogue")]
[SwaggerTag("The Catalogue controller handles Catalogue changes from Struct PIM to Commercetools Categories")]
public class CatalogueController : ControllerBase
{
    private readonly ICategoryService _commerceCategoryService;
    private readonly IErrorService _errorService;
    private readonly ILogger<CatalogueController> _logger;

    public CatalogueController(ILogger<CatalogueController> logger, ICategoryService commerceCategoryService, IErrorService errorService)
    {
        _commerceCategoryService = commerceCategoryService ?? throw new ArgumentNullException(nameof(commerceCategoryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
    public async Task<ActionResult> Webhook([FromBody] CatalogueWebhookModel? model)
    {
        if (!Request.Headers.TryGetValue("X-Event-Key", out var key))
        {
            _logger.LogWarning("X-Event-Key is missing in webhook when trying to handle catalogue {Model}", model?.CatalogueUid);
            return BadRequest("X-Event-Key is missing");
        }

        if (model == null)
        {
            return BadRequest("No model provided");
        }

        return await HandleWebhook(model, key);

    }

    private async Task<ActionResult> HandleWebhook(CatalogueWebhookModel model, StringValues key)
    {

        var webhookEventKey = key.ToString();
        _logger.LogInformation("Handle webhook {Event}", webhookEventKey);
        switch (webhookEventKey)
        {
            case CatalogueWebhookEventKeys.Created:
                await _commerceCategoryService.Create(model.Map());
                return !_errorService.HasErrors() ? Ok() : BadRequest(_errorService.GetErrors());
            case CatalogueWebhookEventKeys.Updated:
                await _commerceCategoryService.Update(model.Map());
                return !_errorService.HasErrors() ? Ok() : BadRequest(_errorService.GetErrors());
            case CatalogueWebhookEventKeys.Deleted:
                await _commerceCategoryService.Delete(model.CatalogueUid);
                return !_errorService.HasErrors() ? Ok() : BadRequest(_errorService.GetErrors());
            default:
                _logger.LogWarning("No handler for webhook {Event}", webhookEventKey);
                return BadRequest($"No handler for webhook {webhookEventKey}");
        }
    }

    [SwaggerOperation(
        Summary = "Create a catalogue",
        Description = "Create a catalogue",
        OperationId = "Create")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ICategory))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [Produces("application/json")]
    [HttpPost]
    public async Task<ActionResult<ICategory?>> Create(CatalogueModel model)
    {
        _logger.LogInformation("Create catalogue with id {Uid}", model.Uid);
        try
        {
            return Ok(await _commerceCategoryService.Create(model));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [SwaggerOperation(
        Summary = "Update a catalogue",
        Description = "Update a catalogue",
        OperationId = "Update")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ICategory))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [Produces("application/json")]
    [HttpPut]
    public async Task<ActionResult<ICategory?>> Update(CatalogueModel model)
    {
        _logger.LogInformation("Update catalogue with id {Uid}", model.Uid);
        try
        {
            return Ok(await _commerceCategoryService.Update(model));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [SwaggerOperation(
        Summary = "Delete a catalogue",
        Description = "Delete a catalogue",
        OperationId = "Delete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ICategory))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [Produces("application/json")]
    [HttpDelete]
    public async Task<ActionResult<ICategory?>> Delete(Guid uid)
    {
        _logger.LogInformation("Delete catalogue with {Uid}", uid);
        try
        {
            return Ok(await _commerceCategoryService.Delete(uid));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}