using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CITests.TestUtilities;
using Flightplan.API.Controllers;
using FluentAssertions;
using FluentAssertions.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using TypeExtensions = CITests.TestUtilities.TypeExtensions;

namespace Struct.PIM.Tests.Struct.PIM.Commercetools;



    [TestClass]
    public class ControllerTests
    {
        private static string PublicNamespace() => "Struct.PIM.Commercetools.Controllers";

        [TestMethod]
        public void Controllers_InPublicNameSpaceShouldBePublic()
        {
            ControllersInPublicNamespace().Should().BeDecoratedWith<RouteAttribute>(e => e.Template.StartsWith("public"));
        }

        [TestMethod]
        public void Controllers_ShouldBeEitherPublicOrPrivate()
        {
            var expectedNamespaces = new List<string> { PublicNamespace() };

            Controllers().Select(e => e.Namespace).Distinct()
                .Should().BeEquivalentTo(expectedNamespaces,
                    opt => opt.WithoutStrictOrdering(),
                    "Controllers should be in either the public or private namespace");
        }

        [TestMethod]
        public void Controllers_Public_ShouldHaveAuthorize()
        {
            ControllersInPublicNamespace().Should().BeDecoratedWith<AuthorizeAttribute>(e => e.AuthenticationSchemes == "api-key");
        }

        [TestMethod]
        public void Controllers_Public_ShouldHaveSwaggerTag()
        {
            ControllersInPublicNamespace().Should().BeDecoratedWith<SwaggerTagAttribute>(e => !string.IsNullOrWhiteSpace(e.Description),
                "Controllers should have a description for ReDoc documentation");
        }

        [TestMethod]
        public void Controllers_PublicMethods_ShouldHaveSwaggerOperation()
        {
            TypeExtensions.Should(PublicMethods()).BeDecoratedWith<SwaggerOperationAttribute>(
                    e => !string.IsNullOrWhiteSpace(e.Description) && !string.IsNullOrWhiteSpace(e.Summary),
                    "Methods should have summary and description for ReDoc documentation");
        }

        [TestMethod]
        public void Controllers_MethodsExceptDelete_ShouldHaveProducesJSON()
        {
            TypeExtensions.Should(PublicMethods()
                    .ThatAreNotDecoratedWith<HttpDeleteAttribute>()).BeDecoratedWith<ProducesAttribute>(e => e.ContentTypes.Contains("application/json"));
        }

        [TestMethod]
        public void Controllers_Methods_ShouldHaveProducesResponseType()
        {
            TypeExtensions.Should(PublicMethods()).BeDecoratedWith<ProducesResponseTypeAttribute>();
        }

        [TestMethod]
        public void Controllers_Methods_ShouldHaveSwaggerResponseExample()
        {
            TypeExtensions.Should(PublicMethods()).BeDecoratedWith<SwaggerResponseExampleAttribute>();
        }

        [TestMethod]
        public void Controllers_MethodsWithoutParameters_ShouldNotHaveSwaggerRequestExample()
        {
            PublicMethods()
                .Where(e => !e.GetParameters().Any())
                .ToArray()
                .Should()
                .NotBeDecoratedWith<SwaggerRequestExampleAttribute>();
        }

        [DynamicData(nameof(GetSwaggerRequestExampleData), DynamicDataSourceType.Method)]
        [DataTestMethod]
        public void Controllers_SwaggerRequestExample_ShouldMatch(MethodInfo method, SwaggerRequestExampleAttribute requestExample)
        {
            var expectedParameters = GetExpectedParameters(method);

            requestExample.RequestType.Should().Be(expectedParameters, "method: {0}", method.Name);

            Type GetExpectedParameters(MethodInfo methodInfo)
            {
                ParameterInfo[] parameters = methodInfo.GetParameters();

                if (methodInfo.GetCustomAttribute(typeof(HttpPostAttribute)) is null)
                {
                    return parameters.Single().ParameterType;
                }

                return parameters.SingleOrDefault(e => e.GetCustomAttribute(typeof(FromBodyAttribute)) is object)?.ParameterType
                    ?? parameters.Single().ParameterType;
            }
        }

        public static IEnumerable<object[]> GetSwaggerRequestExampleData()
        {
            return PublicMethods()
                .Where(e => e.GetParameters().Any())
                .SelectMany(e => e.GetCustomAttributes<SwaggerRequestExampleAttribute>(), (item, value) => (item, value))
                .Select(e => new object[] { e.item, e.value });
        }

        [DynamicData(nameof(GetSwaggerResponseExampleData), DynamicDataSourceType.Method)]
        [DataTestMethod]
        public void Controllers_SwaggerResponseExample_ShouldMatch(MethodInfo method, SwaggerResponseExampleAttribute responseExample)
        {
            Type expected = ExtractReturnType(method.ReturnType);
            var actual = GetExampleType(responseExample.ExamplesProviderType);

            actual.Should().Be(expected, "method: {0}", method.Name);
        }

        public static IEnumerable<object[]> GetSwaggerResponseExampleData()
        {
            return PublicMethods()
                .SelectMany(e => e.GetCustomAttributes<SwaggerResponseExampleAttribute>()
                                .Where(f => f.StatusCode == StatusCodes.Status200OK),
                                (item, value) => (item, value))
                .Select(e => new object[] { e.item, e.value });
        }

        [DynamicData(nameof(GetProducesResponseTypeData), DynamicDataSourceType.Method)]
        [DataTestMethod]
        public void Controllers_ProducesResponseType_ShouldHaveSwaggerResponseExample(MethodInfo method, ProducesResponseTypeAttribute producesResponseType)
        {
            var expectedType = GetExpectedType(method, producesResponseType);
            method.Should().BeDecoratedWith<SwaggerResponseExampleAttribute>(e => GetExampleType(e.ExamplesProviderType) == expectedType,
                "method should have an example for {0}", expectedType);

            Type GetExpectedType(MethodInfo methodInfo, ProducesResponseTypeAttribute producesResponseTypeAttribute)
            {
                return producesResponseTypeAttribute.Type == typeof(void)
                    ? ExtractReturnType(methodInfo.ReturnType)
                    : producesResponseTypeAttribute.Type;
            }
        }

        public static IEnumerable<object[]> GetProducesResponseTypeData()
        {
            return PublicMethods()
                .SelectMany(e => e.GetCustomAttributes<ProducesResponseTypeAttribute>(),
                                (item, value) => (item, value))
                .Select(e => new object[] { e.item, e.value });
        }

        private static Type ExtractReturnType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                type = type.GetGenericArguments().Single();
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ActionResult<>))
            {
                type = type.GetGenericArguments().Single();
            }

            return type;
        }

        private static Type GetExampleType(Type type)
        {
            return type
                .GetInterfaces()
                .Single(e => e.IsGenericType)
                .GetGenericArguments()[0];
        }

        [DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
        [DataTestMethod]
        public void Controllers_Methods_ShouldHaveExamples(MethodInfo method)
        {
            var responseTypeStatusCodes = method
                .GetCustomAttributes<ProducesResponseTypeAttribute>()
                .Select(e => e.StatusCode)
                .Distinct();

            var exampleStatusCodes = method
                .GetCustomAttributes<SwaggerResponseExampleAttribute>()
                .Select(e => e.StatusCode)
                .Distinct();

            exampleStatusCodes.Should().BeEquivalentTo(responseTypeStatusCodes,
                opt => opt.WithoutStrictOrdering(),
                "there should be examples for all types of status codes");
        }

        public static IEnumerable<object[]> GetData() => PublicMethods().Select(e => new[] { e });

        private static MethodInfoSelector PublicMethods()
        {
            return ControllersInPublicNamespace()
                .Methods()
                .ThatArePublicOrInternal; // Fluent Assertions does not have a 'ThatArePublic';
        }

        private static TypeSelector ControllersInPublicNamespace() => Controllers().ThatAreInNamespace(PublicNamespace());

        private static TypeSelector Controllers()
        {
            return AllTypes.From(typeof(AircraftController).Assembly)
                .ThatDeriveFrom<ControllerBase>()
                .Types();
        }
    }
}
