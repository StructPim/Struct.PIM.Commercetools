using commercetools.Sdk.Api.Models.Projects;

namespace Struct.PIM.Commercetools.Services.Commercetools.Interfaces;

public interface IProjectSettingsService
{
    Task<IProject?> CreateLanguages();
}