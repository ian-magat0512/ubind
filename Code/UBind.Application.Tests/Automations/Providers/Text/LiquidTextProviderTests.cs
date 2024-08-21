// <copyright file="LiquidTextProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Text
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DotLiquid;
    using DotLiquid.NamingConventions;
    using FluentAssertions;
    using Moq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Object;
    using UBind.Application.Automation.PathLookup;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Liquid;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Automation.Providers.Text;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class LiquidTextProviderTests
    {
        public LiquidTextProviderTests()
        {
            // Configure DotLiquid as per Startup.cs
            this.ConfigureDotLiquid();
        }

        [Fact]
        public async Task Resolve_CanRenderText_WithDataObjectFromAutomationDataUsingPrimitives()
        {
            // Arrange
            var textTemplate = "Let's say {{fooNumber}}";
            var data = await MockAutomationData.CreateWithHttpTrigger(withTriggerContent: true);
            var liquidTextProvider = this.CreateProvider(textTemplate, false, "/trigger/httpRequest/content");

            // Act
            var renderedText = (await liquidTextProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            renderedText.ToString().Should().Be("Let's say 18");
        }

        [Fact]
        public async Task Resolve_ShouldRenderText_FromItemInAList()
        {
            // Arrange
            var textTemplate = "Let's say {{items[0]}}";
            var data = await MockAutomationData.CreateWithHttpTrigger(withTriggerContent: true);
            var liquidTextProvider = this.CreateProvider(textTemplate, false, "/trigger/httpRequest/content");

            // Act
            var renderedText = (await liquidTextProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            renderedText.ToString().Should().Be("Let's say a1");
        }

        [Fact]
        public async Task Resolve_ShouldRenderText_FromItemInAList_Using_Variable()
        {
            // Arrange
            var textTemplate = "Let's say {{items[index]}}";
            var data = await MockAutomationData.CreateWithHttpTrigger(withTriggerContent: true);
            var liquidTextProvider = this.CreateProvider(textTemplate, false, "/trigger/httpRequest/content");

            // Act
            var renderedText = (await liquidTextProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            renderedText.ToString().Should().Be("Let's say a1");
        }

        [Fact]
        public async Task Resolve_ShouldRenderText_FromObjectInAList()
        {
            // Arrange
            var textTemplate = "Let's say {{persons[0].name}}";
            var data = await MockAutomationData.CreateWithHttpTrigger(withTriggerContent: true);
            var liquidTextProvider = this.CreateProvider(textTemplate, false, "/trigger/httpRequest/content");

            // Act
            var renderedText = (await liquidTextProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            renderedText.ToString().Should().Be("Let's say B1");
        }

        [Fact]
        public async Task Resolve_ShouldRenderText_FromItemInAListWithInAList()
        {
            // Arrange
            var textTemplate = "Let's say {{persons[1].items[0]}}";
            var data = await MockAutomationData.CreateWithHttpTrigger(withTriggerContent: true);
            var liquidTextProvider = this.CreateProvider(textTemplate, false, "/trigger/httpRequest/content");

            // Act
            var renderedText = (await liquidTextProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            renderedText.ToString().Should().Be("Let's say x1");
        }

        [Fact]
        public async Task Resolve_ShouldRenderText_FromItemInAListWithInAList_Using_Variable()
        {
            // Arrange
            var textTemplate = "Let's say {{persons[index].items[index]}}";
            var data = await MockAutomationData.CreateWithHttpTrigger(withTriggerContent: true);
            var liquidTextProvider = this.CreateProvider(textTemplate, false, "/trigger/httpRequest/content");

            // Act
            var renderedText = (await liquidTextProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            renderedText.ToString().Should().Be("Let's say p1");
        }

        [Fact]
        public async Task Resolve_ShouldRenderText_WhenIteratingItemInTheList()
        {
            // Arrange
            var textTemplate = @"{% for person in persons %}{{ person.name }}{% endfor %}";
            var data = await MockAutomationData.CreateWithHttpTrigger(withTriggerContent: true);
            var liquidTextProvider = this.CreateProvider(textTemplate, false, "/trigger/httpRequest/content");

            // Act
            var renderedText = (await liquidTextProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            renderedText.ToString().Should().Be("B1B2");
        }

        [Fact]
        public async Task Resolve_ShouldRenderText_WhenIteratingItemInTheListWithInAList()
        {
            // Arrange
            var textTemplate = @"{% for item in persons[0].items %}{{item}}{% endfor %}";
            var data = await MockAutomationData.CreateWithHttpTrigger(withTriggerContent: true);
            var liquidTextProvider = this.CreateProvider(textTemplate, false, "/trigger/httpRequest/content");

            // Act
            var renderedText = (await liquidTextProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            renderedText.ToString().Should().Be("p1p2p3");
        }

        [Fact]
        public async Task Resolve_ShouldReturnErrorMessage_WhenIndexOutOfRange()
        {
            // Arrange
            var textTemplate = "Let's say {{items[100]}}";
            var data = await MockAutomationData.CreateWithHttpTrigger(withTriggerContent: true);
            var liquidTextProvider = this.CreateProvider(textTemplate, false, "/trigger/httpRequest/content");

            // Act
            var renderedText = (await liquidTextProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            renderedText.ToString().Should().Contain("Index was out of range.");
        }

        [Fact]
        public async Task Resolve_ShouldReturnEmptyString_WhenIndexIsNOTNumber()
        {
            // Arrange
            var textTemplate = "Let's say {{items[\"a\"]}}";
            var data = await MockAutomationData.CreateWithHttpTrigger(withTriggerContent: true);
            var liquidTextProvider = this.CreateProvider(textTemplate, false, "/trigger/httpRequest/content");

            // Act
            var renderedText = (await liquidTextProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            renderedText.ToString().Should().Be("Let's say ");
        }

        [Fact]
        public async Task Resolve_CanRenderText_WithDynamicDataObject()
        {
            // Assert
            var data = await MockAutomationData.CreateWithHttpTrigger();
            var liquidTextProvider = this.CreateProvider();

            // Act
            var renderedText = (await liquidTextProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            renderedText.ToString().Should().Be("Hi bazz9");
        }

        [Fact]
        public async Task Resolve_ShouldThrowException_IfLiquidTemplateIsEmptyText()
        {
            var data = await MockAutomationData.CreateWithHttpTrigger();
            var liquidtextProvider = this.CreateProvider(string.Empty);

            // Act & Assert
            Func<Task> action = async () => (await liquidtextProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();
            await action.Should().ThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task Resolve_CanRenderText_WithRemoveFilter()
        {
            // Arrange
            var textTemplate = "Let's say {{ '$32,276' | Remove: '$' | Remove: ',' }}";
            var data = await MockAutomationData.CreateWithHttpTrigger();
            var liquidTextProvider = this.CreateProvider(textTemplate);

            // Act
            var renderedText = (await liquidTextProvider.Resolve(new ProviderContext(data))).GetValueOrThrowIfFailed();

            // Assert
            renderedText.ToString().Should().Be("Let's say 32276");
        }

        private IProvider<Data<string>> CreateProvider(string template = null, bool useDynamicProperties = true, string objectPath = null)
        {
            var stringTemplate = template ?? "Hi {{foo1}}";
            var fixedTextTemplate = new StaticBuilder<Data<string>> { Value = stringTemplate };
            LiquidTextProviderConfigModel providerModel;
            if (!useDynamicProperties)
            {
                var fixedTextProviderConfigModel = new StaticBuilder<Data<string>> { Value = objectPath };
                var fixedObjectPathLookupConfigModel = new FixedObjectPathLookupConfigModel { Path = fixedTextProviderConfigModel };
                var pathLookupTextProviderConfigModel = new PathLookupTextProviderConfigModel { PathLookup = fixedObjectPathLookupConfigModel };
                var jsonObjectProviderConfigModel = new JsonTextToObjectProviderConfigModel { TextProvider = pathLookupTextProviderConfigModel };
                providerModel = new LiquidTextProviderConfigModel { LiquidTemplate = fixedTextTemplate, DataObject = jsonObjectProviderConfigModel };
            }
            else
            {
                List<ObjectPropertyConfigModel> dynamicProperties = new List<ObjectPropertyConfigModel>();
                for (var a = 0; a < 5; a++)
                {
                    dynamicProperties.Add(new ObjectPropertyConfigModel
                    {
                        PropertyName = new StaticBuilder<Data<string>> { Value = $"foo{a}" },
                        Value = new StaticBuilder<IData> { Value = new Data<object>($"bazz{10 - a}") },
                    });
                }

                var dynamicObjectModelTemplate = new DynamicObjectProviderConfigModel() { Properties = dynamicProperties };
                providerModel = new LiquidTextProviderConfigModel { LiquidTemplate = fixedTextTemplate, DataObject = dynamicObjectModelTemplate };
            }

            return providerModel.Build(new Mock<IServiceProvider>().AddLoggers().Object);
        }

        private void ConfigureDotLiquid()
        {
            // Set the naming convention to CSharpNamingConvention to match the conventions used in our Liquid templates.
            Template.NamingConvention = new CSharpNamingConvention();
            Template.RegisterTag<RenderTag>("render");
        }
    }
}
