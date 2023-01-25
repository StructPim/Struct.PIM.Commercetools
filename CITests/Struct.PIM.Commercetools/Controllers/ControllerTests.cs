using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Types;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using Struct.PIM.Commercetools.Controllers;
using Swashbuckle.AspNetCore.Annotations;
using TypeExtensions = CITests.Utilities.TypeExtensions;

namespace CITests.Struct.PIM.Commercetools.Controllers;

public class ControllerTests
{
    private static string PublicNamespace()
    {
        return "Struct.PIM.Commercetools.Controllers";
    }

    [Test]
    public void Controllers_ShouldBeEitherPublicOrPrivate()
    {
        var expectedNamespaces = new List<string> { PublicNamespace() };

        Controllers().Select(e => e.Namespace).Distinct()
            .Should().BeEquivalentTo(expectedNamespaces,
                opt => opt.WithoutStrictOrdering(),
                "Controllers should be in either the public or private namespace");
    }

    [Test]
    public void Controllers_Public_ShouldHaveSwaggerTag()
    {
        ControllersInPublicNamespace().Should().BeDecoratedWith<SwaggerTagAttribute>(e => !string.IsNullOrWhiteSpace(e.Description),
            "Controllers should have a description");
    }

    [Test]
    public void Controllers_PublicMethods_ShouldHaveSwaggerOperation()
    {
        TypeExtensions.Should(PublicMethods()).BeDecoratedWith<SwaggerOperationAttribute>(
            e => !string.IsNullOrWhiteSpace(e.Description) && !string.IsNullOrWhiteSpace(e.Summary),
            "Methods should have summary and description ");
    }

    [Test]
    public void Controllers_ShouldHaveProducesJSON()
    {
        TypeExtensions.Should(PublicMethods())
            .BeDecoratedWith<ProducesAttribute>(e => e.ContentTypes.Contains("application/json"));
    }

    [Test]
    public void Controllers_Methods_ShouldHaveProducesResponseType()
    {
        TypeExtensions.Should(PublicMethods()).BeDecoratedWith<ProducesResponseTypeAttribute>();
    }

    private static MethodInfoSelector PublicMethods()
    {
        return ControllersInPublicNamespace()
            .Methods()
            .ThatArePublicOrInternal; // Fluent Assertions does not have a 'ThatArePublic';
    }

    private static TypeSelector ControllersInPublicNamespace()
    {
        return Controllers().ThatAreInNamespace(PublicNamespace());
    }

    private static TypeSelector Controllers()
    {
        return AllTypes.From(typeof(CatalogueController).Assembly)
            .ThatDeriveFrom<ControllerBase>()
            .Types();
    }
}