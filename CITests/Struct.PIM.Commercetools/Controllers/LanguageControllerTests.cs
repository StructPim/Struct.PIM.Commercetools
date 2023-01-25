using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using commercetools.Sdk.Api.Models.Projects;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Struct.PIM.Api.Client.Endpoints.Interfaces;
using Struct.PIM.Api.Models.Language;
using Struct.PIM.Commercetools.Controllers;
using Struct.PIM.Commercetools.Services;
using Struct.PIM.Commercetools.Services.Commercetools.Interfaces;
using Struct.PIM.Commercetools.Webhook.EventKeys;
using Struct.PIM.WebhookModels;

namespace CITests.Struct.PIM.Commercetools.Controllers;

[TestFixture]
public class LanguageControllerTests
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
        var logger = _fixture.Freeze<Mock<ILogger<LanguageController>>>();
        var languageEndpoint = _fixture.Freeze<Mock<ILanguageEndpoint>>();
        var projectSettingsService = _fixture.Freeze<Mock<IProjectSettingsService>>();
        var errorService = new Mock<IErrorService>();
        var controller = new LanguageController(logger.Object, languageEndpoint.Object, projectSettingsService.Object, errorService.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
        
        // Act
        var result = await controller.Webhook(new LanguageWebhookModel());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        result.As<BadRequestObjectResult>().Value.Should().Be("X-Event-Key is missing");
    }

    [Test]
    public async Task Webhook_ShouldReturn_Ok([Values(LanguageWebhookEventKeys.Created, LanguageWebhookEventKeys.Updated)] string eventKey)
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<LanguageController>>>();
        var languageEndpoint = _fixture.Freeze<Mock<ILanguageEndpoint>>();
        languageEndpoint.Setup(p => p.GetLanguagesAsync()).ReturnsAsync(_fixture.Create<List<LanguageModel>>());
        var projectSettingsService = _fixture.Freeze<Mock<IProjectSettingsService>>();
        projectSettingsService.Setup(p => p.CreateLanguages()).ReturnsAsync(It.IsAny<IProject>());
        var errorService = new Mock<IErrorService>();
        errorService.Setup(p => p.HasErrors()).Returns(false);
        var controller = new LanguageController(logger.Object, languageEndpoint.Object, projectSettingsService.Object, errorService.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
        controller.ControllerContext.HttpContext.Request.Headers.Add("X-Event-Key", eventKey);
        // Act
        var result = await controller.Webhook(_fixture.Create<LanguageWebhookModel>());

        // Assert
        result.Should().BeOfType<OkResult>();
    }
    [Test]
    public async Task Webhook_ShouldReturn_BadRequest([Values(LanguageWebhookEventKeys.Created, LanguageWebhookEventKeys.Updated)] string eventKey)
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<LanguageController>>>();
        var languageEndpoint = _fixture.Freeze<Mock<ILanguageEndpoint>>();
        languageEndpoint.Setup(p => p.GetLanguagesAsync()).ReturnsAsync(_fixture.Create<List<LanguageModel>>());
        var projectSettingsService = _fixture.Freeze<Mock<IProjectSettingsService>>();
        projectSettingsService.Setup(p => p.CreateLanguages()).ReturnsAsync(It.IsAny<IProject>());
        var errorService = new Mock<IErrorService>();
        errorService.Setup(p => p.HasErrors()).Returns(true);
        var controller = new LanguageController(logger.Object, languageEndpoint.Object, projectSettingsService.Object, errorService.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
        controller.ControllerContext.HttpContext.Request.Headers.Add("X-Event-Key", eventKey);

        // Act
        var result = await controller.Webhook(_fixture.Create<LanguageWebhookModel>());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    } 
    
    [Test]
    public async Task Webhook_NoModel_ShouldReturn_BadRequest()
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<LanguageController>>>();
        var languageEndpoint = _fixture.Freeze<Mock<ILanguageEndpoint>>();
        var projectSettingsService = _fixture.Freeze<Mock<IProjectSettingsService>>();
        var errorService = new Mock<IErrorService>();
        var controller = new LanguageController(logger.Object, languageEndpoint.Object, projectSettingsService.Object, errorService.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
        controller.ControllerContext.HttpContext.Request.Headers.Add("X-Event-Key", LanguageWebhookEventKeys.Created);

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
        var logger = _fixture.Freeze<Mock<ILogger<LanguageController>>>();
        var languageEndpoint = _fixture.Freeze<Mock<ILanguageEndpoint>>();
        languageEndpoint.Setup(p => p.GetLanguagesAsync()).ReturnsAsync(_fixture.Create<List<LanguageModel>>());
        var projectSettingsService = _fixture.Freeze<Mock<IProjectSettingsService>>();
        projectSettingsService.Setup(p => p.CreateLanguages()).ReturnsAsync(It.IsAny<IProject>());
        var errorService = new Mock<IErrorService>();
        var controller = new LanguageController(logger.Object, languageEndpoint.Object, projectSettingsService.Object, errorService.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
        var eventKey = "language:no_handler";
        controller.ControllerContext.HttpContext.Request.Headers.Add("X-Event-Key", eventKey);
        
        // Act
        var result = await controller.Webhook(_fixture.Create<LanguageWebhookModel>());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        result.As<BadRequestObjectResult>().Value.Should().Be($"No handler for webhook {eventKey}");
    }
    [Test]
    public async Task Webhook_Delete_ShouldReturn_BadRequest()
    {
        // Arrange
        var logger = _fixture.Freeze<Mock<ILogger<LanguageController>>>();
        var languageEndpoint = _fixture.Freeze<Mock<ILanguageEndpoint>>();
        languageEndpoint.Setup(p => p.GetLanguagesAsync()).ReturnsAsync(_fixture.Create<List<LanguageModel>>());
        var projectSettingsService = _fixture.Freeze<Mock<IProjectSettingsService>>();
        projectSettingsService.Setup(p => p.CreateLanguages()).ReturnsAsync(It.IsAny<IProject>());
        var errorService = new Mock<IErrorService>();
        var controller = new LanguageController(logger.Object, languageEndpoint.Object, projectSettingsService.Object, errorService.Object)
        {
            ControllerContext = { HttpContext = new DefaultHttpContext() }
        };
        controller.ControllerContext.HttpContext.Request.Headers.Add("X-Event-Key", LanguageWebhookEventKeys.Deleted);

        // Act
        var result = await controller.Webhook(_fixture.Create<LanguageWebhookModel>());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        result.As<BadRequestObjectResult>().Value.Should().Be("Deleting language in Commercetools not supported");
    } 
    
}