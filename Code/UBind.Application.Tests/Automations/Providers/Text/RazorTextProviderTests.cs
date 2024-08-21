// <copyright file="RazorTextProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Text
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using Newtonsoft.Json.Linq;
    using RazorEngine.Templating;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Object;
    using UBind.Application.Automation.PathLookup;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Automation.Providers.Text;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class RazorTextProviderTests
    {
        [Fact]
        public async Task Resolve_CanRenderTextFromDynamicObject_Successfully()
        {
            // Arrange
            var data = await MockAutomationData.CreateWithHttpTrigger();
            var razorTextProvider = this.CreateProvider();

            // Act
            var renderedText = (await razorTextProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            renderedText.ToString().Should().Be("Hi bazz9");
        }

        [Fact]
        public async Task Resolve_CanRenderTextFromObjectInsideObject_Successfully()
        {
            var data = await MockAutomationData.CreateWithHttpTrigger(withTriggerContent: true);
            var razorTextProvider = this.CreateProvider("@Model.greeting", false, "/trigger/httpRequest/content");

            // Act
            var renderedText = (await razorTextProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            renderedText.ToString().Should().Be("Hi World!");
        }

        [Fact]
        public async Task Resolve_ThrowsExceptionWithDetails_WhenTextCannotBeRendered()
        {
            // Arrange
            var data = await MockAutomationData.CreateWithHttpTrigger();
            var razorTextProvider = this.CreateProvider("@Model.foo");

            // Act
            Func<Task> func = async () => await razorTextProvider.Resolve(new ProviderContext(data));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            var expectedErrorReceived = Errors.Automation.ValueResolutionError(razorTextProvider.SchemaReferenceKey, new JObject());
            exception.Which.Message.Should().Contain(expectedErrorReceived.Message);
            exception.Which.Error.Code.Should().Be(expectedErrorReceived.Code);
            exception.Which.Error.Data.Should().NotBeNull();
        }

        private IProvider<Data<string>> CreateProvider(string template = null, bool useDynamicProperties = true, string objectPath = null)
        {
            var stringTemplate = template ?? @"Hi @Model.foo1";
            IProvider<Data<string>> razorTemplate = new StaticProvider<Data<string>>(stringTemplate);
            IObjectProvider dataObjectProvider;
            if (!useDynamicProperties)
            {
                var staticTextProviderConfigModel = new StaticBuilder<Data<string>> { Value = objectPath };
                var fixedObjectPathLookupConfigModel = new FixedObjectPathLookupConfigModel { Path = staticTextProviderConfigModel };
                var pathLookupTextProviderConfigModel = new PathLookupTextProviderConfigModel { PathLookup = fixedObjectPathLookupConfigModel };
                var jsonObjectProviderConfigModel = new JsonTextToObjectProviderConfigModel { TextProvider = pathLookupTextProviderConfigModel };
                dataObjectProvider = jsonObjectProviderConfigModel.Build(null);
            }
            else
            {
                var dynamicProperties = new List<ObjectPropertyConfigModel>();
                for (var a = 0; a < 5; a++)
                {
                    dynamicProperties.Add(new ObjectPropertyConfigModel
                    {
                        PropertyName = new StaticBuilder<Data<string>> { Value = $"foo{a}" },
                        Value = new StaticBuilder<IData> { Value = new Data<object>($"bazz{10 - a}") },
                    });
                }

                var dynamicObjectModelTemplate = new DynamicObjectProviderConfigModel() { Properties = dynamicProperties };
                dataObjectProvider = dynamicObjectModelTemplate.Build(new Mock<IServiceProvider>().AddLoggers().Object);
            }

            var cachingResolverMock = new Mock<ICachingResolver>();
            cachingResolverMock.Setup(s => s.GetTenantOrThrow(It.IsAny<Guid>())).ReturnsAsync(TenantFactory.Create());

            var razorEngine = RazorEngineService.Create();
            var razorTextProvider = new RazorTextProvider(
                razorTemplate,
                dataObjectProvider,
                razorEngine,
                NullLogger<RazorTextProvider>.Instance,
                cachingResolverMock.Object);
            return razorTextProvider;
        }
    }
}
