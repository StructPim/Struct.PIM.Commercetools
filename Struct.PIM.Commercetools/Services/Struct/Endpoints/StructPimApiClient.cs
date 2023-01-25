using Struct.PIM.Api.Client;

namespace Struct.PIM.Commercetools.Services.Struct.Endpoints;

public interface IStructPimApiClient
{
    StructPIMApiClient Client { get; }
}

public class StructPimApiClient : IStructPimApiClient
{
    public StructPimApiClient(StructPIMApiClient client)
    {
        Client = client;
    }

    public StructPIMApiClient Client { get; }
}