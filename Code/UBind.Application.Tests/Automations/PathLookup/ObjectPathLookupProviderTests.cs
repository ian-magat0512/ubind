// <copyright file="ObjectPathLookupProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.PathLookup
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.PathLookup;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using Xunit;

    [SystemEventTypeExtensionInitialize]
    public class ObjectPathLookupProviderTests
    {
        [Fact]
        public async Task Resolve_ReturnsCorrectData_WhenPathsUseIndexes()
        {
            // Arrange
            var pathProvider = new StaticProvider<Data<string>>("/foo/bar/2");
            var jsonProvider = new StaticProvider<Data<string>>(@"{
    ""foo"": {
        ""bar"": [
            ""baz1"",
            ""baz2"",
            ""baz3""
        ]
    }
}");
            var objectProvider = new JsonTextToObjectProvider(jsonProvider);
            var sut = new ObjectPathLookupProvider(
                pathProvider,
                objectProvider,
                "objectPathLookupObject");

            // Act
            string result = (await sut.Resolve(new ProviderContext(MockAutomationData.CreateWithEventTrigger()))).GetValueOrThrowIfFailed() as Data<string>;

            // Assert
            result.Should().Be("baz3");
        }

        [Fact]
        public async Task Resolve_ReturnsCorrectData_WhenPathAccessesObjectInsideArray()
        {
            // Arrange
            var pathProvider = new StaticProvider<Data<string>>("/foo/bar/1/baz");
            var jsonProvider = new StaticProvider<Data<string>>(@"{
    ""foo"": {
        ""bar"": [
            {
                ""baz"": 1,
            },
            {
                ""baz"": 2
            },
        ]
    }
}");
            var objectProvider = new JsonTextToObjectProvider(jsonProvider);
            var sut = new ObjectPathLookupProvider(
                pathProvider,
                objectProvider,
                "objectPathLookupInteger");

            // Act
            long result = (await sut.Resolve(new ProviderContext(MockAutomationData.CreateWithEventTrigger()))).GetValueOrThrowIfFailed() as Data<long>;

            // Assert
            result.Should().Be(2);
        }

        [Fact]
        public async Task Resolve_ThrowsErrorException_WhenPathSegmentNotFoundAsync()
        {
            // Arrange
            var pathProvider = new StaticProvider<Data<string>>("/foo/baz");
            var jsonProvider = new StaticProvider<Data<string>>(@"{
    ""foo"": {
        ""bar"": 1
    }
}");
            var objectProvider = new JsonTextToObjectProvider(jsonProvider);
            var sut = new ObjectPathLookupProvider(
                pathProvider,
                objectProvider,
                "objectPathLookupInteger");

            // Act
            Func<Task> func = async () => (await sut.Resolve(new ProviderContext(MockAutomationData.CreateWithEventTrigger()))).GetValueOrThrowIfFailed();

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("automation.providers.path.not.found");
        }

        [Fact]
        public async Task Resolve_ThrowsErrorException_WhenPathIntermediateSegmentNotObjectAsync()
        {
            // Arrange
            var pathProvider = new StaticProvider<Data<string>>("/foo/bar/qux");
            var jsonProvider = new StaticProvider<Data<string>>(@"{
    ""foo"": {
        ""bar"": 1,
        ""baz"": {
            ""qux"": 2
        }    
    }
}");
            var objectProvider = new JsonTextToObjectProvider(jsonProvider);
            var sut = new ObjectPathLookupProvider(
                pathProvider,
                objectProvider,
                "objectPathLookupInteger");

            // Act
            Func<Task> func = async () => await sut.Resolve(new ProviderContext(MockAutomationData.CreateWithEventTrigger()));

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("automation.providers.path.resolves.to.primitive.when.object.or.array.expected");
        }

        [Fact]
        public async Task Resolve_ThrowsErrorException_WhenIndexOutOfRangeAsync()
        {
            // Arrange
            var pathProvider = new StaticProvider<Data<string>>("/foo/bar/3/baz");
            var jsonProvider = new StaticProvider<Data<string>>(@"{
    ""foo"": {
        ""bar"": [
            {
                ""baz"": 1,
            },
            {
                ""baz"": 2
            },
        ]
    }
}");
            var objectProvider = new JsonTextToObjectProvider(jsonProvider);
            var sut = new ObjectPathLookupProvider(
                pathProvider,
                objectProvider,
                "objectPathLookupInteger");

            // Act
            Func<Task> func = async () => await sut.Resolve(new ProviderContext(MockAutomationData.CreateWithEventTrigger()));

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("automation.providers.path.index.out.of.range");
        }
    }
}
