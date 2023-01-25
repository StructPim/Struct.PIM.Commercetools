using commercetools.Base.Client;
using commercetools.Sdk.Api.Client.RequestBuilders.Projects;
using commercetools.Sdk.Api.Extensions;
using commercetools.Sdk.Api.Models.Projects;
using Struct.PIM.Api.Client.Endpoints.Interfaces;
using Struct.PIM.Commercetools.Helpers;
using Struct.PIM.Commercetools.Services.Commercetools.Interfaces;

namespace Struct.PIM.Commercetools.Services.Commercetools;

public class ProjectSettingsService : IProjectSettingsService
{
    private readonly ByProjectKeyRequestBuilder _builder;
    private readonly IErrorService _errorService;
    private readonly ILogger<ProjectSettingsService> _logger;
    private readonly ILanguageEndpoint _structLanguageEndpoint;

    public ProjectSettingsService(IClient client, ILogger<ProjectSettingsService> logger, ILanguageEndpoint structLanguageEndpoint, IErrorService errorService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _builder = client != null
            ? client.WithApi().WithProjectKey(Settings.CommerceProjectKey)
            : throw new ArgumentNullException(nameof(client));
        _structLanguageEndpoint = structLanguageEndpoint ?? throw new ArgumentNullException(nameof(structLanguageEndpoint));
        _errorService = errorService ?? throw new ArgumentNullException(nameof(errorService));
    }

    /// <summary>
    ///     Creates a Language in commercetools
    /// </summary>
    public async Task<IProject?> CreateLanguages()
    {
        var languages = (await _structLanguageEndpoint.GetLanguagesAsync())?.Select(p => LocalizeHelper.GetTwoLetterIsoLanguageName(p.CultureCode));
        var currentProject = await _builder.Get().ExecuteAsync();
        var diff = languages?.Except(currentProject.Languages);
        if (diff == null)
        {
            return null;
        }

        var update = new ProjectUpdate
        {
            Version = currentProject.Version,
            Actions = new List<IProjectUpdateAction>
            {
                new ProjectChangeLanguagesAction
                {
                    Languages = diff.ToList()
                }
            }
        };
        try
        {
            var project = await _builder.Post(update).ExecuteAsync();
            _logger.LogInformation("Update language in project {Project}", Settings.CommerceProjectKey);
            return project;
        }
        catch (Exception e)
        {
            const string err = "Failed to update the languages in Commercetools.";
            _logger.LogError("{Err} The error was {Message}", err, e.ResolveMessage());
            _errorService.AddError(err);
            return null;
        }
    }
}