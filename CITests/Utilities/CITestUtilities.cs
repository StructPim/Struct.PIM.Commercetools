using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using Moq.Protected;

namespace CITests.Utilities;

// ReSharper disable once InconsistentNaming
public static class CITestUtilities
{
    public static HttpClientMockBuilder CreateMockedHttpClientFactory(this IFixture fixture, string baseAddress, string clientName = "StructApiClient")
    {
        var mockBuilder = new HttpClientMockBuilder(clientName, new Uri(baseAddress));
        fixture.Inject(mockBuilder.Build());
        return mockBuilder;
    }

    public class HttpClientMockBuilder
    {
        private readonly IHttpClientFactory _factory;
        private readonly Mock<HttpMessageHandler> _handler;

        public HttpClientMockBuilder(string name, Uri baseAddress)
        {
            _handler = new Mock<HttpMessageHandler>();
            var factory = new Mock<IHttpClientFactory>();
            var client = new HttpClient(_handler.Object);
            client.BaseAddress = baseAddress;
            _factory = factory.Object;
            factory.Setup(x => x.CreateClient(name)).Returns(client);
        }

        public HttpClientMockBuilder WithResponse(string requestUrl, HttpStatusCode statusCode, HttpContent content)
        {
            _handler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(x => x.RequestUri != null && x.RequestUri.OriginalString == requestUrl),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = content
                })
                .Verifiable();

            return this;
        }

        public IHttpClientFactory Build()
        {
            return _factory;
        }
    }
}