using commercetools.Sdk.Api.Models.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Struct.PIM.Api.Client.Endpoints.Interfaces;
using Struct.PIM.Api.Models.Catalogue;
using Struct.PIM.Commercetools.Services;
using Struct.PIM.Commercetools.Services.Commercetools.Interfaces;
using Struct.PIM.Commercetools.Webhook.EventKeys;
using Struct.PIM.WebhookModels;
using Swashbuckle.AspNetCore.Annotations;

namespace Struct.PIM.Commercetools.Controllers;

[SwaggerTag("The Category controller handles Category changes from Struct PIM to Commercetools Categories")]
[ApiController]
[Route("category")]
public class CategoryController : ControllerBase
{
    private readonly ICatalogueEndpoint _catalogueEndpoint;
    private readonly ICategoryService _commerceCategoryService;
    private readonly IErrorService _errorService;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(ILogger<CategoryController> logger, ICategoryService commerceCategoryService, ICatalogueEndpoint catalogueEndpoint, IErrorService errorService)
    {
        _commerceCategoryService = commerceCategoryService ?? throw new ArgumentNullException(nameof(commerceCategoryService));
        _catalogueEndpoint = catalogueEndpoint ?? throw new ArgumentNullException(nameof(catalogueEndpoint));
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
    public async Task<ActionResult> Webhook(CategoryWebhookModel? model)
    {
        if (!Request.Headers.TryGetValue("X-Event-Key", out var key))
        {
            _logger.LogInformation("X-Event-Key is missing in webhook when trying to handle categories {Model}", model?.CategoryIds);
            return BadRequest("X-Event-Key is missing");
        }

        if (model == null)
        {
            return BadRequest("No model provided");
        }

        return await HandleWebhook(model, key);
    }

    private async Task<ActionResult> HandleWebhook(CategoryWebhookModel model, StringValues key)
    {
        var webhookEventKey = key.ToString();
        _logger.LogInformation("Handle webhook {Event}", webhookEventKey);
        if (model.CategoryIds == null || !model.CategoryIds.Any())
        {
            return BadRequest("No category ids provided");
        }


        var categories = await _catalogueEndpoint.GetCategoriesAsync(model.CategoryIds.ToList());
        if (categories.Any())
        {
            switch (webhookEventKey)
            {
                case CategoryWebhookEventKeys.Created:
                    await _commerceCategoryService.Create(categories);
                    return !_errorService.HasErrors() ? Ok() : BadRequest(_errorService.GetErrors());
                case CategoryWebhookEventKeys.Updated:
                    await _commerceCategoryService.Update(categories);
                    return !_errorService.HasErrors() ? Ok() : BadRequest(_errorService.GetErrors());
                case CategoryWebhookEventKeys.Deleted:
                    await _commerceCategoryService.Delete(categories);
                    return !_errorService.HasErrors() ? Ok() : BadRequest(_errorService.GetErrors());
                default:
                    _logger.LogWarning("No handler for webhook {Event}", webhookEventKey);
                    return BadRequest($"No handler for webhook {webhookEventKey}");
            }
        }

        _logger.LogInformation("No matching Struct categories found for {Ids}", model.CategoryIds.Select(x => x));
        return BadRequest($"No matching Struct categories found for {model.CategoryIds.Select(x => x)}");
    }


    [SwaggerOperation(
        Summary = "Get catalogue Uid",
        Description = "Return the Struct catalogue Uid for the given category id",
        OperationId = "CatalogueId")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ICategory))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [Produces("application/json")]
    [HttpGet]
    public async Task<ActionResult<Guid?>> CatalogueUid(int categoryId)
    {
        try
        {
            var category = await _catalogueEndpoint.GetCategoryAsync(categoryId);
            return Ok(category?.CatalogueUid);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [SwaggerOperation(
        Summary = "Create a category",
        Description = "Create a category",
        OperationId = "Create")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ICategory))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [Produces("application/json")]
    [HttpPost]
    public async Task<ActionResult<ICategory?>> Create(CategoryModel model)
    {
        _logger.LogInformation("Create category with id {Id}", model.Id);
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
        Summary = "Update a category",
        Description = "Update a category",
        OperationId = "Update")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ICategory))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [Produces("application/json")]
    [HttpPut]
    public async Task<ActionResult<ICategory?>> Update(CategoryModel model)
    {
        _logger.LogInformation("Update category with id {Id}", model.Id);
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
        Summary = "Delete a category",
        Description = "Delete a category",
        OperationId = "Delete")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ICategory))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [Produces("application/json")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ICategory?>> Delete(int id)
    {
        _logger.LogInformation("Delete category with {Id}", id);
        try
        {
            return Ok(await _commerceCategoryService.Delete(id));
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}