using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using commercetools.Sdk.Api.Models.Categories;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Struct.PIM.Api.Models.Catalogue;
using Struct.PIM.Commercetools.Controllers;
using Struct.PIM.Commercetools.Services;
using Struct.PIM.Commercetools.Services.Commercetools.Interfaces;
using Struct.PIM.Commercetools.Webhook.EventKeys;
using Struct.PIM.WebhookModels;

namespace CITests.Struct.PIM.Commercetools.Controllers;

[TestFixture]
public class CatalogueControllerTests
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
        var logger = _fixture.Freeze<Mock<ILogger<CatalogueController>>>();
        var categoryService = _fixture.Freeze<Mock<ICategoryService>>();
        var errorService = new Mock<IErrorService>();
        var controller = new CatalogueController(logger.Object, categoryService.Object, errorService.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };

        // Act
        var result = await controller.Webhook(new CatalogueWebhookModel());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        result.As<BadRequestObjectResult>().Value.Should().Be("X-Event-Key is missing");
    }

    [Test]
    public async Task Webhook_NoModel_ShouldReturn_BadRequest()
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<CatalogueController>>>();
        var categoryService = _fixture.Freeze<Mock<ICategoryService>>();
         var errorService = new Mock<IErrorService>();
        var controller = new CatalogueController(logger.Object, categoryService.Object,errorService.Object);
        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        controller.ControllerContext.HttpContext.Request.Headers.Add("X-Event-Key", CatalogueWebhookEventKeys.Created);

        // Act
        var result = await controller.Webhook(It.IsAny<CatalogueWebhookModel>());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        result.As<BadRequestObjectResult>().Value.Should().Be("No model provided");
    }

    [Test]
    public async Task Webhook_NoHandler_ShouldReturn_BadRequest()
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<CatalogueController>>>();
        var categoryService = _fixture.Freeze<Mock<ICategoryService>>();
        var errorService = new Mock<IErrorService>();
        var controller = new CatalogueController(logger.Object, categoryService.Object,errorService.Object);
        var eventKey = "catalogue:no_handler";
        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        controller.ControllerContext.HttpContext.Request.Headers.Add("X-Event-Key", eventKey);

        // Act
        var result = await controller.Webhook(new CatalogueWebhookModel());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        result.As<BadRequestObjectResult>().Value.Should().Be($"No handler for webhook {eventKey}");
    }

    [Test]
    public async Task Webhook_CategoryServiceFails_ShouldReturn_BadRequest()
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<CatalogueController>>>();
        var categoryService = new Mock<ICategoryService>();
        var errorService = new Mock<IErrorService>();
        var errMessage = new List<string>() { "Error message #1", "Error message #2" };
        categoryService.Setup(p =>
            p.Create(It.IsAny<CatalogueModel>())
        ).ReturnsAsync(It.IsAny<ICategory>());
        errorService.Setup(p => p.HasErrors()).Returns(true);
        errorService.Setup(p => p.GetErrors()).Returns(errMessage);

        var controller = new CatalogueController(logger.Object, categoryService.Object,errorService.Object)
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.ControllerContext.HttpContext.Request.Headers.Add("X-Event-Key", CatalogueWebhookEventKeys.Created);

        // Act
        var result = await controller.Webhook(new CatalogueWebhookModel());

        // Assert
        ((List<string>)result.As<ObjectResult>().Value!).Should().Equal(errMessage).And.HaveCount(2);
        result.As<ObjectResult>().StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task Webhook_ShouldReturn_Ok()
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<CatalogueController>>>();
        var categoryService = _fixture.Freeze<Mock<ICategoryService>>();
        var controller = new CatalogueController(logger.Object, categoryService.Object,new ErrorService())
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.ControllerContext.HttpContext.Request.Headers.Add("X-Event-Key", CatalogueWebhookEventKeys.Created);

        // Act
        var result = await controller.Webhook(new CatalogueWebhookModel());

        // Assert
        result.Should().BeOfType<OkResult>();
    }


    [Test]
    public async Task Create_ShouldReturn_Ok()
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<CatalogueController>>>();
        var categoryService = _fixture.Freeze<Mock<ICategoryService>>();
        var controller = new CatalogueController(logger.Object, categoryService.Object,new ErrorService());

        // Act
        var result = await controller.Create(new CatalogueModel());

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task Create_ShouldReturn_BadRequest()
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<CatalogueController>>>();
        var categoryService = _fixture.Freeze<Mock<ICategoryService>>();
        var errMessage = "Error message";
        categoryService.Setup(p =>
            p.Create(It.IsAny<CatalogueModel>())
        ).ThrowsAsync(new Exception(errMessage));
        var controller = new CatalogueController(logger.Object, categoryService.Object,new ErrorService());

        // Act
        var result = await controller.Create(new CatalogueModel());

        // Assert
        result.Result.As<ObjectResult>().Value.Should().Be(errMessage);
        result.Result.As<ObjectResult>().StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task Update_ShouldReturn_Ok()
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<CatalogueController>>>();
        var categoryService = _fixture.Freeze<Mock<ICategoryService>>();
        var controller = new CatalogueController(logger.Object, categoryService.Object,new ErrorService());

        // Act
        var result = await controller.Update(new CatalogueModel());

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task Update_ShouldReturn_BadRequest()
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<CatalogueController>>>();
        var categoryService = _fixture.Freeze<Mock<ICategoryService>>();
        var errMessage = "Error message";
        categoryService.Setup(p =>
            p.Update(It.IsAny<CatalogueModel>())
        ).ThrowsAsync(new Exception(errMessage));
        var controller = new CatalogueController(logger.Object, categoryService.Object,new ErrorService());

        // Act
        var result = await controller.Update(new CatalogueModel());

        // Assert
        result.Result.As<ObjectResult>().Value.Should().Be(errMessage);
        result.Result.As<ObjectResult>().StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Test]
    public async Task Delete_ShouldReturn_Ok()
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<CatalogueController>>>();
        var categoryService = _fixture.Freeze<Mock<ICategoryService>>();
        var controller = new CatalogueController(logger.Object, categoryService.Object,new ErrorService());

        // Act
        var result = await controller.Delete(It.IsAny<Guid>());

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task Delete_ShouldReturn_BadRequest()
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<CatalogueController>>>();
        var categoryService = _fixture.Freeze<Mock<ICategoryService>>();
        var errMessage = "Error message";
        categoryService.Setup(p =>
            p.Delete(It.IsAny<Guid>())
        ).ThrowsAsync(new Exception(errMessage));
        var controller = new CatalogueController(logger.Object, categoryService.Object,new ErrorService());

        // Act
        var result = await controller.Delete(It.IsAny<Guid>());

        // Assert
        result.Result.As<ObjectResult>().Value.Should().Be(errMessage);
        result.Result.As<ObjectResult>().StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }
    
     [Test]
    public async Task Webhook_ShouldReturn_Ok([Values(CatalogueWebhookEventKeys.Created, CatalogueWebhookEventKeys.Updated, CatalogueWebhookEventKeys.Deleted)] string eventKey)
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<CatalogueController>>>();
        var categoryService = _fixture.Freeze<Mock<ICategoryService>>();
        var errorService = new Mock<IErrorService>();
        errorService.Setup(p => p.HasErrors()).Returns(false);
        var controller = new CatalogueController(logger.Object, categoryService.Object,errorService.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
        controller.ControllerContext.HttpContext.Request.Headers.Add("X-Event-Key", eventKey);
        // Act
        var result = await controller.Webhook(_fixture.Create<CatalogueWebhookModel>());

        // Assert
        result.Should().BeOfType<OkResult>();
    }
    
    [Test]
    public async Task Webhook_ShouldReturn_BadRequest([Values(CatalogueWebhookEventKeys.Created, CatalogueWebhookEventKeys.Updated, CatalogueWebhookEventKeys.Deleted)] string eventKey)
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<CatalogueController>>>();
        var categoryService = _fixture.Freeze<Mock<ICategoryService>>();
        var errorService = new Mock<IErrorService>();
        errorService.Setup(p => p.HasErrors()).Returns(true);
        var controller = new CatalogueController(logger.Object, categoryService.Object,errorService.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
        controller.ControllerContext.HttpContext.Request.Headers.Add("X-Event-Key", eventKey);

        // Act
        var result = await controller.Webhook(_fixture.Create<CatalogueWebhookModel>());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    } 
}