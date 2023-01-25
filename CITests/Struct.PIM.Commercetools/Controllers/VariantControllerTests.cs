using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Struct.PIM.Api.Client.Endpoints.Interfaces;
using Struct.PIM.Api.Models.Variant;
using Struct.PIM.Commercetools.Controllers;
using Struct.PIM.Commercetools.Services;
using Struct.PIM.Commercetools.Services.Commercetools.Interfaces;
using Struct.PIM.Commercetools.Webhook.EventKeys;
using Struct.PIM.WebhookModels;

namespace CITests.Struct.PIM.Commercetools.Controllers;

[TestFixture]
public class VariantControllerTests
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
        var logger = _fixture.Freeze<Mock<ILogger<VariantController>>>();
        var variantEndpoint = _fixture.Freeze<Mock<IVariantEndpoint>>();
        var productVariantService = _fixture.Freeze<Mock<IProductVariantService>>();
        var errorService = new Mock<IErrorService>();
        var controller = new VariantController(logger.Object, variantEndpoint.Object, productVariantService.Object, errorService.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
        
        // Act
        var result = await controller.Webhook(new VariantWebhookModel());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        result.As<BadRequestObjectResult>().Value.Should().Be("X-Event-Key is missing");
    }

    [Test]
    public async Task Webhook_ShouldReturn_Ok([Values(VariantWebhookEventKeys.Created, VariantWebhookEventKeys.Updated, VariantWebhookEventKeys.Deleted)] string eventKey)
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<VariantController>>>();
        var productEndpoint = _fixture.Freeze<Mock<IVariantEndpoint>>();
        productEndpoint.Setup(p => p.GetVariantsAsync(It.IsAny<List<int>>())).ReturnsAsync(_fixture.Create<List<VariantModel>>());
        var productVariantService = _fixture.Freeze<Mock<IProductVariantService>>();
        productVariantService.Setup(p => p.CreateVariants(It.IsAny<List<VariantModel>>())).Returns(Task.FromResult((object)null!));
        productVariantService.Setup(p => p.UpdateVariants(It.IsAny<List<VariantModel>>())).Returns(Task.FromResult((object)null!));
        productVariantService.Setup(p => p.DeleteVariants(It.IsAny<List<VariantModel>>())).Returns(Task.FromResult((object)null!));
        var errorService = new Mock<IErrorService>();
        errorService.Setup(p => p.HasErrors()).Returns(false);
        var controller = new VariantController(logger.Object, productEndpoint.Object, productVariantService.Object, errorService.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
        controller.ControllerContext.HttpContext.Request.Headers.Add("X-Event-Key", eventKey);
        // Act
        var result = await controller.Webhook(_fixture.Create<VariantWebhookModel>());

        // Assert
        result.Should().BeOfType<OkResult>();
    }
    [Test]
    public async Task Webhook_ShouldReturn_BadRequest([Values(VariantWebhookEventKeys.Created, VariantWebhookEventKeys.Updated, VariantWebhookEventKeys.Deleted)] string eventKey)
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<VariantController>>>();
        var productEndpoint = _fixture.Freeze<Mock<IVariantEndpoint>>();
        productEndpoint.Setup(p => p.GetVariantsAsync(It.IsAny<List<int>>())).ReturnsAsync(_fixture.Create<List<VariantModel>>());
        var productVariantService = _fixture.Freeze<Mock<IProductVariantService>>();
        productVariantService.Setup(p => p.CreateVariants(It.IsAny<List<VariantModel>>())).Returns(Task.FromResult((object)null!));
        productVariantService.Setup(p => p.UpdateVariants(It.IsAny<List<VariantModel>>())).Returns(Task.FromResult((object)null!));
        productVariantService.Setup(p => p.DeleteVariants(It.IsAny<List<VariantModel>>())).Returns(Task.FromResult((object)null!));
        var errorService = new Mock<IErrorService>();
        errorService.Setup(p => p.HasErrors()).Returns(true);
        var controller = new VariantController(logger.Object, productEndpoint.Object, productVariantService.Object, errorService.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
        controller.ControllerContext.HttpContext.Request.Headers.Add("X-Event-Key", eventKey);
        // Act
        var result = await controller.Webhook(_fixture.Create<VariantWebhookModel>());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
   
    [Test]
    public async Task Webhook_NoModel_ShouldReturn_BadRequest()
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<VariantController>>>();
        var productEndpoint = _fixture.Freeze<Mock<IVariantEndpoint>>();
        var productVariantService = _fixture.Freeze<Mock<IProductVariantService>>();
        var errorService = new Mock<IErrorService>();
        var controller = new VariantController(logger.Object, productEndpoint.Object, productVariantService.Object, errorService.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
        controller.ControllerContext.HttpContext.Request.Headers.Add("X-Event-Key", VariantWebhookEventKeys.Created);
    
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
        var logger = _fixture.Freeze<Mock<ILogger<VariantController>>>();
        var productEndpoint = _fixture.Freeze<Mock<IVariantEndpoint>>();
        productEndpoint.Setup(p => p.GetVariantsAsync(It.IsAny<List<int>>())).ReturnsAsync(_fixture.Create<List<VariantModel>>());
        var productVariantService = _fixture.Freeze<Mock<IProductVariantService>>();
        var errorService = new Mock<IErrorService>();
        errorService.Setup(p => p.HasErrors()).Returns(true);
        var controller = new VariantController(logger.Object, productEndpoint.Object, productVariantService.Object, errorService.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
        var eventKey = "language:no_handler";
        controller.ControllerContext.HttpContext.Request.Headers.Add("X-Event-Key", eventKey);
        
        // Act
        var result = await controller.Webhook(_fixture.Create<VariantWebhookModel>());
    
        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        result.As<BadRequestObjectResult>().Value.Should().Be($"No handler for webhook {eventKey}");
    }
}