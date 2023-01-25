using commercetools.Base.Client;
using commercetools.Base.Client.Error;
using commercetools.Sdk.Api.Client.RequestBuilders.Projects;
using commercetools.Sdk.Api.Extensions;
using commercetools.Sdk.Api.Models.Categories;
using Struct.PIM.Api.Client.Endpoints.Interfaces;
using Struct.PIM.Api.Models.Catalogue;
using Struct.PIM.Commercetools.Helpers;
using Struct.PIM.Commercetools.Mapping;
using Struct.PIM.Commercetools.Services.Commercetools.Interfaces;

namespace Struct.PIM.Commercetools.Services.Commercetools;

public class CategoryService : ICategoryService
{
    private readonly ByProjectKeyRequestBuilder _builder;
    private readonly IErrorService _errorService;
    private readonly ILogger _logger;

    public CategoryService(ILogger<CategoryService> logger, IClient client, ICatalogueEndpoint catalogueEndpoint, IErrorService errorService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _builder = client != null
            ? client.WithApi().WithProjectKey(Settings.CommerceProjectKey)
            : throw new ArgumentNullException(nameof(client));
        _errorService = errorService ?? throw new ArgumentNullException(nameof(errorService));
    }


    /// <summary>
    ///     Creates a category in commercetools
    /// </summary>
    /// <param name="catalogueModel"></param>
    public async Task<ICategory?> Create(CatalogueModel catalogueModel)
    {
        var model = catalogueModel.MapDraft();
        return await Create(model);
    }

    /// <summary>
    ///     Creates a categories in commercetools
    /// </summary>
    /// <param name="catalogueModels"></param>
    public async Task Create(List<CatalogueModel> catalogueModels)
    {
        var drafts = catalogueModels.Select(p => Create(p.MapDraft())).ToList();
        while (drafts.Any())
        {
            var finishedTask = await Task.WhenAny(drafts);
            drafts.Remove(finishedTask);
        }

    }

    /// <summary>
    ///     Creates a category in commercetools
    /// </summary>
    /// <param name="categoryModel"></param>
    public async Task<ICategory?> Create(CategoryModel categoryModel)
    {
        var model = categoryModel.MapDraft();
        return await Create(model);
    }

    /// <summary>
    ///     Creates a categories in commercetools
    /// </summary>
    /// <param name="categoryModels"></param>
    public async Task Create(List<CategoryModel> categoryModels)
    {
        foreach (var category in categoryModels.OrderBy(categoryModel => categoryModel.Id))
        {
            await Create(category);
        }
    }

    /// <summary>
    ///     Updates a category in commercetools
    /// </summary>
    /// <param name="catalogueModel"></param>
    public async Task<ICategory?> Update(CatalogueModel catalogueModel)
    {
        var key = catalogueModel.Uid.ToCommerceKey();
        var existing = await GetByKey(key);
        if (existing == null)
        {
            _logger.LogWarning("Skipping updating catalogue, {Key}, not found in Commercetools", key);
            return null;
        }

        var actions = catalogueModel.GetChanges(existing);
        if (actions == null)
        {
            _logger.LogInformation("Skipping updating catalogue, {Key}, since there is no changes", key);
            return null;
        }

        try
        {
            var category = await _builder.Categories().WithKey(key).Post(actions).ExecuteAsync();
            _logger.LogInformation("Update catalogue {Uid}", catalogueModel.Uid);
            return category;
        }
        catch (Exception e)
        {
            var err = $"Failed to update the catalogue {key} in Commercetools.";
            _logger.LogError("{Err} The error was {Message}", err, e.ResolveMessage());
            _errorService.AddError(err);
            return null;
        }
    }

    /// <summary>
    ///     Updates categories in commercetools
    /// </summary>
    /// <param name="catalogueModels"></param>
    public async Task Update(List<CatalogueModel> catalogueModels)
    {
        var drafts = catalogueModels.Select(Update).ToList();

        while (drafts.Any())
        {
            var finishedTask = await Task.WhenAny(drafts);
            drafts.Remove(finishedTask);
        }
    }

    /// <summary>
    ///     Updates a category in commercetools
    /// </summary>
    /// <param name="categoryModel"></param>
    public async Task<ICategory?> Update(CategoryModel categoryModel)
    {
        var key = categoryModel.Id.ToCommerceKey();
        var existing = await GetByKey(key);
        if (existing == null)
        {
            _logger.LogWarning("Skipping updating category, {Key}, not found in Commercetools", key);
            return null;
        }

        var parent = await GetByKey(existing.Parent.Id);

        var actions = categoryModel.GetChanges(existing, parent);
        if (actions == null)
        {
            _logger.LogInformation("Skipping updating category, {Key}, since there is no changes", key);
            return null;
        }

        try
        {
            var category = await _builder.Categories().WithKey(key).Post(actions).ExecuteAsync();
            _logger.LogInformation("Update category {Key}", key);
            return category;
        }
        catch (Exception e)
        {
            var err = $"Failed to update the category {key} in Commercetools.";
            _logger.LogError("{Err} The error was {Message}", err, e.ResolveMessage());
            _errorService.AddError(err);
            return null;
        }
    }

    /// <summary>
    ///     Updates categories in commercetools
    /// </summary>
    /// <param name="categoryModels"></param>
    public async Task Update(List<CategoryModel> categoryModels)
    {
        var drafts = categoryModels.Select(Update).ToList();

        while (drafts.Any())
        {
            var finishedTask = await Task.WhenAny(drafts);
            drafts.Remove(finishedTask);
        }
    }

    /// <summary>
    ///     Deletes a category in commercetools
    /// </summary>
    /// <param name="id">The Struct Category Id</param>
    public async Task<ICategory?> Delete(int id)
    {
        return await DeleteByKey(id.ToCommerceKey());
    }

    /// <summary>
    ///     Updates categories in commercetools
    /// </summary>
    /// <param name="ids"></param>
    public async Task Delete(List<int> ids)
    {
        var drafts = ids.Select(Delete).ToList();

        while (drafts.Any())
        {
            var finishedTask = await Task.WhenAny(drafts);
            drafts.Remove(finishedTask);
        }

    }


    /// <summary>
    ///     Deletes a category in commercetools
    /// </summary>
    /// <param name="categoryModels"></param>
    public async Task Delete(List<CategoryModel> categoryModels)
    {
        var ids = categoryModels.Select(p => p.Id).OrderByDescending(id => id).Select(p => p.ToCommerceKey());
        foreach (var id in ids)
        {
            await DeleteByKey(id);
        }
    }


    /// <summary>
    ///     Deletes a categories in commercetools
    /// </summary>
    /// <param name="guids">The Struct Category/Catalogue Guids</param>
    public async Task Delete(List<Guid> guids)
    {
        foreach (var uid in guids)
        {
            await DeleteByKey(uid.ToCommerceKey());
        }
    }

    /// <summary>
    ///     Deletes a category in commercetools
    /// </summary>
    /// <param name="id">The Struct Category/Catalogue Guid</param>
    public async Task<ICategory?> Delete(Guid id)
    {
        return await DeleteByKey(id.ToCommerceKey());
    }


    /// <summary>
    ///     Get a category by key from Commercetools
    /// </summary>
    /// <param name="key"></param>
    private async Task<ICategory?> GetByKey(string key)
    {
        try
        {
            var category = await _builder.Categories().WithKey(key.ToCommerceKey()).Get().ExecuteAsync();
            return category;
        }
        catch (NotFoundException)
        {
            _logger.LogWarning("Category {Key}, not found in Commercetools", key);
            return null;
        }
        catch (BadRequestException e)
        {
            _logger.LogWarning("Failed to get the category {Key}, in Commercetools. The error was {Error}", key, e.ResolveMessage());
            return null;
        }
    }

    private async Task<ICategory?> Create(ICategoryDraft categoryDraft)
    {
        try
        {
            var category = await _builder.Categories().Post(categoryDraft).ExecuteAsync();
            _logger.LogInformation("Create catalogue {Key}", categoryDraft.Key);
            return category;

        }
        catch (Exception e)
        {
            var err = $"Failed to create the category {categoryDraft.Key}, in Commercetools.";
            _logger.LogError("{Err} The error was {Error}", err, e.ResolveMessage());
            _errorService.AddError(err);
            return null;
        }
    }

    /// <summary>
    ///     Deletes a category in commercetools
    /// </summary>
    /// <param name="id">The Struct Category/Catalogue Id</param>
    public async Task<ICategory?> Delete(string id)
    {
        return await DeleteByKey(id.ToCommerceKey());
    }

    private async Task<ICategory?> DeleteByKey(string key)
    {
        var existing = await GetByKey(key);
        if (existing == null)
        {
            _logger.LogWarning("Could not delete the category {Key}, in Commercetools since it does not exist", key);
            return null;
        }

        try
        {
            var category = await _builder.Categories().WithKey(key).Delete().WithVersion(existing.Version).ExecuteAsync();
            _logger.LogInformation("Delete category {Key}", key);
            return category;
        }
        catch (Exception e)
        {
            var err = $"Failed to delete the category {key}, in Commercetools.";
            _logger.LogError("{Err} The error was {Error}", err, e.ResolveMessage());
            _errorService.AddError(err);
            return null;
        }
    }
}