namespace Struct.PIM.Commercetools.Services.Struct.Endpoints;

public class Endpoint
{
    protected readonly HttpClient StructClient;

    protected Endpoint(IHttpClientFactory httpClientFactory)
    {
        StructClient = httpClientFactory.CreateClient("StructApiClient");
    }
}