// <copyright file="PathLookupListProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.List
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Object;
    using UBind.Application.Automation.PathLookup;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.List;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class PathLookupListProviderTests
    {
        private IServiceProvider serviceProvider = new Mock<IServiceProvider>().AddLoggers().Object;
        private List<object> listValues = new List<object> { "item1", "item2", "item3" };

        [Fact]
        public async Task PathLookupListProvider_Should_ResolveListFromInlineValues()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var dataObjectDictionary = new List<ObjectPropertyConfigModel>()
            {
                new ObjectPropertyConfigModel
                {
                    PropertyName = new StaticBuilder<Data<string>>() { Value = "listValues" },
                    Value = (IBuilder<IProvider<IData>>)new StaticBuilder<Data<List<object>>>() { Value = this.listValues },
                },
            };
            var dataObjectProvider = new DynamicObjectProviderConfigModel() { Properties = dataObjectDictionary };
            var pathProvider = new StaticBuilder<Data<string>> { Value = "/listValues" };
            var objectPathLookupProvider = new ObjectPathLookupProviderConfigModel() { Path = pathProvider, DataObject = dataObjectProvider };
            var pathLookupBuilder = new PathLookupListProviderConfigModel(objectPathLookupProvider, null, null, null, null, null, null, null);
            var pathLookupProvider = pathLookupBuilder.Build(this.serviceProvider);

            // Act
            var resolvedList = await pathLookupProvider.Resolve(new ProviderContext(automationData));
            var list = resolvedList.GetValueOrThrowIfFailed().ToList();

            // Assert
            list.Should().NotBeNull();
            list.Should().HaveCount(3);
            var item = list[0].ToString();
            item.Should().Be(this.listValues[0].ToString());
        }

        [Fact]
        public async Task PathLookupListProvider_Should_ThrowErrorIfResolvedValueIsNotAList()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
            var pathProvider = new StaticBuilder<Data<string>> { Value = "trigger.httpRequest.content" };
            var defaultList = new GenericDataList<object>(this.listValues);
            var defaultProvider = new StaticListBuilder<object> { Value = defaultList };
            var fixedPathLookupProvider = new FixedObjectPathLookupConfigModel { Path = pathProvider };
            var pathLookupBuilder = new PathLookupListProviderConfigModel(fixedPathLookupProvider, defaultProvider, null, null, null, null, null, null);
            var pathLookupProvider = pathLookupBuilder.Build(this.serviceProvider);

            // Act
            Func<Task> func = async () => await pathLookupProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var expectedError = Errors.Automation.PathQueryValueInvalidType(pathLookupProvider.SchemaReferenceKey, "list", nameof(PathLookupListProvider), null);
            var result = await func.Should().ThrowAsync<ErrorException>();
            result.Which.Error.Title.Should().Be(expectedError.Title);
            result.Which.Error.Code.Should().Be(expectedError.Code);
        }

        [Fact]
        public async Task PathLookupListProvider_Should_UseDefaultValueIfPresent()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
            var pathProvider = new StaticBuilder<Data<string>> { Value = "trigger.httpRequest.content.listValues" };
            var defaultList = new GenericDataList<object>(this.listValues);
            var defaultProvider = new StaticListBuilder<object> { Value = defaultList };
            var fixedPathLookupProvider = new FixedObjectPathLookupConfigModel { Path = pathProvider };
            var pathLookupBuilder = new PathLookupListProviderConfigModel(fixedPathLookupProvider, defaultProvider, null, null, null, null, null, null);
            var pathLookupProvider = pathLookupBuilder.Build(this.serviceProvider);

            // Act
            var resolvedList = await pathLookupProvider.Resolve(new ProviderContext(automationData));
            var list = resolvedList.GetValueOrThrowIfFailed().ToList();

            // Assert
            list.Should().NotBeNull();
            list.Should().HaveCount(3);
            list.Should().BeEquivalentTo(this.listValues);
        }
    }
}
