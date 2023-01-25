using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using commercetools.Sdk.Api.Models.ProductTypes;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Struct.PIM.Api.Client.Endpoints.Interfaces;
using Struct.PIM.Api.Models.ProductStructure;
using Struct.PIM.Commercetools.Controllers;
using Struct.PIM.Commercetools.Services;
using Struct.PIM.Commercetools.Services.Commercetools.Interfaces;
using Struct.PIM.Commercetools.Webhook.EventKeys;
using Struct.PIM.WebhookModels;

namespace CITests.Struct.PIM.Commercetools.Controllers;

[TestFixture]
public class ProductStructureControllerTests
{
    private IFixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture().Customize(new AutoMoqCustomization());
    }

    [Test]
    public async Task Webhook_XEventKeyMissing_ShouldReturn_BadRequest()
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<ProductStructureController>>>();
        var productStructureEndpoint = _fixture.Freeze<Mock<IProductStructureEndpoint>>();
        var productTypeService = _fixture.Freeze<Mock<IProductTypeService>>();
        var configuration = _fixture.Freeze<Mock<IConfiguration>>();
        var errorService = new Mock<IErrorService>();
        var controller = new ProductStructureController(logger.Object, productStructureEndpoint.Object, productTypeService.Object, configuration.Object, errorService.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
        
        // Act
        var result = await controller.Webhook(new ProductStructureWebhookModel());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        result.As<BadRequestObjectResult>().Value.Should().Be("X-Event-Key is missing");
    }

    [Test]
    public async Task Webhook_ShouldReturn_Ok([Values(ProductStructureWebhookEventKeys.Created)] string eventKey)
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<ProductStructureController>>>();
        var productStructureEndpoint = _fixture.Freeze<Mock<IProductStructureEndpoint>>();
        productStructureEndpoint.Setup(p => p.GetProductStructureAsync(It.IsAny<Guid>())).ReturnsAsync(new ProductStructure());
        var productTypeService = _fixture.Freeze<Mock<IProductTypeService>>();
        productTypeService.Setup(p => p.Create(It.IsAny<ProductStructure>(), It.IsAny<List<string>?>())).ReturnsAsync(It.IsAny<IProductType>());
        var configuration = _fixture.Freeze<Mock<IConfiguration>>();
        var errorService = new Mock<IErrorService>();
        errorService.Setup(p => p.HasErrors()).Returns(false);
        var controller = new ProductStructureController(logger.Object, productStructureEndpoint.Object, productTypeService.Object, configuration.Object, errorService.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
        controller.ControllerContext.HttpContext.Request.Headers.Add("X-Event-Key", eventKey);
        // Act
        var result = await controller.Webhook(_fixture.Create<ProductStructureWebhookModel>());

        // Assert
        result.Should().BeOfType<OkResult>();
    }
    [Test]
    public async Task Webhook_ShouldReturn_BadRequest([Values(ProductStructureWebhookEventKeys.Created,ProductStructureWebhookEventKeys.Updated, ProductStructureWebhookEventKeys.Deleted)] string eventKey)
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<ProductStructureController>>>();
        var productStructureEndpoint = _fixture.Freeze<Mock<IProductStructureEndpoint>>();
        productStructureEndpoint.Setup(p => p.GetProductStructureAsync(It.IsAny<Guid>())).ReturnsAsync(new ProductStructure());
        var productTypeService = _fixture.Freeze<Mock<IProductTypeService>>();
        productTypeService.Setup(p => p.Create(It.IsAny<ProductStructure>(), It.IsAny<List<string>?>())).ReturnsAsync(It.IsAny<IProductType>());
        var configuration = _fixture.Freeze<Mock<IConfiguration>>();
        var errorService = new Mock<IErrorService>();
        errorService.Setup(p => p.HasErrors()).Returns(true);
        var controller = new ProductStructureController(logger.Object, productStructureEndpoint.Object, productTypeService.Object, configuration.Object, errorService.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
        controller.ControllerContext.HttpContext.Request.Headers.Add("X-Event-Key", eventKey);
        // Act
        var result = await controller.Webhook(_fixture.Create<ProductStructureWebhookModel>());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
    
    
    [Test]
    public async Task Webhook_NoModel_ShouldReturn_BadRequest()
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<ProductStructureController>>>();
        var productStructureEndpoint = _fixture.Freeze<Mock<IProductStructureEndpoint>>();
        var productTypeService = _fixture.Freeze<Mock<IProductTypeService>>();
        var configuration = _fixture.Freeze<Mock<IConfiguration>>();
        var errorService = new Mock<IErrorService>();
        var controller = new ProductStructureController(logger.Object, productStructureEndpoint.Object, productTypeService.Object, configuration.Object, errorService.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
        controller.ControllerContext.HttpContext.Request.Headers.Add("X-Event-Key", ProductWebhookEventKeys.Created);
    
        // Act
        var result = await controller.Webhook(null);
    
        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        result.As<BadRequestObjectResult>().Value.Should().Be("No model provided");
    } 
    [Test]
    public async Task Webhook_NoHandler_ShouldReturn_BadRequest()
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<ProductStructureController>>>();
        var productStructureEndpoint = _fixture.Freeze<Mock<IProductStructureEndpoint>>();
        productStructureEndpoint.Setup(p => p.GetProductStructureAsync(It.IsAny<Guid>())).ReturnsAsync(new ProductStructure());
        var productTypeService = _fixture.Freeze<Mock<IProductTypeService>>();
        productTypeService.Setup(p => p.Create(It.IsAny<ProductStructure>(), It.IsAny<List<string>?>())).ReturnsAsync(It.IsAny<IProductType>());
        var configuration = _fixture.Freeze<Mock<IConfiguration>>();
        var errorService = new Mock<IErrorService>();
        errorService.Setup(p => p.HasErrors()).Returns(true);
        var controller = new ProductStructureController(logger.Object, productStructureEndpoint.Object, productTypeService.Object, configuration.Object, errorService.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
        var eventKey = "language:no_handler";
        controller.ControllerContext.HttpContext.Request.Headers.Add("X-Event-Key", eventKey);
        
        // Act
        var result = await controller.Webhook(_fixture.Create<ProductStructureWebhookModel>());
    
        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        result.As<BadRequestObjectResult>().Value.Should().Be($"No handler for webhook {eventKey}");
    }
}