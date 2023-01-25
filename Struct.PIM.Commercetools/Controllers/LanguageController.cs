using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Struct.PIM.Api.Client.Endpoints.Interfaces;
using Struct.PIM.Commercetools.Services;
using Struct.PIM.Commercetools.Services.Commercetools.Interfaces;
using Struct.PIM.Commercetools.Webhook.EventKeys;
using Struct.PIM.WebhookModels;
using Swashbuckle.AspNetCore.Annotations;

namespace Struct.PIM.Commercetools.Controllers;

[SwaggerTag("The Language controller handles Language changes from Struct PIM to Commercetools Languages")]
[ApiController]
[Route("languages")]
public class LanguageController : ControllerBase
{
    private readonly IErrorService _errorService;
    private readonly ILanguageEndpoint _languageEndpoint;
    private readonly ILogger<LanguageController> _logger;
    private readonly IProjectSettingsService _projectSettingsService;

    public LanguageController(ILogger<LanguageController> logger, ILanguageEndpoint languageEndpoint, IProjectSettingsService projectSettingsService, IErrorService errorService)
    {
        _languageEndpoint = languageEndpoint ?? throw new ArgumentNullException(nameof(languageEndpoint));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _projectSettingsService = projectSettingsService ?? throw new ArgumentNullException(nameof(projectSettingsService));
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
    public async Task<ActionResult> Webhook([FromBody] LanguageWebhookModel? model)
    {
        if (!Request.Headers.TryGetValue("X-Event-Key", out var key))
        {
            _logger.LogWarning("X-Event-Key is missing in webhook when trying to handle language {Model}", model?.LanguageId);
            return BadRequest("X-Event-Key is missing");
        }

        if (model == null)
        {
            return BadRequest("No model provided");
        }

        return await HandleWebhook(key);
    }

    private async Task<ActionResult> HandleWebhook(StringValues key)
    {

        var webhookEventKey = key.ToString();
        _logger.LogInformation("Handle webhook {Event}", webhookEventKey);

        var languages = await _languageEndpoint.GetLanguagesAsync();
        if (languages.Any())
        {
            switch (webhookEventKey)
            {
                case LanguageWebhookEventKeys.Created:
                    await _projectSettingsService.CreateLanguages();
                    return !_errorService.HasErrors() ? Ok() : BadRequest(_errorService.GetErrors());
                case LanguageWebhookEventKeys.Updated:
                    await _projectSettingsService.CreateLanguages();
                    return !_errorService.HasErrors() ? Ok() : BadRequest(_errorService.GetErrors());
                case LanguageWebhookEventKeys.Deleted:
                    return BadRequest("Deleting language in Commercetools not supported");
                default:
                    _logger.LogInformation("No handler for webhook {Event}", webhookEventKey);
                    return BadRequest($"No handler for webhook {webhookEventKey}");
            }
        }

        _logger.LogWarning("No languages defined in Struct PIM");
        return BadRequest("No languages defined in Struct PIM");
    }
}