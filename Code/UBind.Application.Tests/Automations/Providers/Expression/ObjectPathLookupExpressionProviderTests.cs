// <copyright file="ObjectPathLookupExpressionProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Expression
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using Xunit;

    [SystemEventTypeExtensionInitialize]
    public class ObjectPathLookupExpressionProviderTests
    {
        [Theory]
        [InlineData("#/a/simple/x", ".Simple.X")]
        [InlineData("#/a/compoundPropertyName/x", ".CompoundPropertyName.X")]
        public async Task Resolve_FindsCorrectProperty_WhenPathUsesCamelCaseAsync(string path, string expectedExpression)
        {
            // Arrange
            var parameterExpression = Expression.Parameter(typeof(Foo));
            var scope = new ExpressionScope("a", parameterExpression);
            var pathProvider = new StaticProvider<Data<string>>(path);
            var sut = new ObjectPathLookupExpressionProvider<long>(pathProvider);

            // Act
            var result = await sut.Invoke(new ProviderContext(MockAutomationData.CreateWithEventTrigger()), scope);

            // Assert
            result.ToString().Should().EndWith(expectedExpression);
        }

        [Theory]
        [InlineData("#/a/Simple/x")]
        [InlineData("#/a/simple/X")]
        [InlineData("#/a/CompoundPropertyName/x")]
        [InlineData("#/a/compoundPropertyName/X")]
        public async Task Resolve_ThrowsErrorException_WhenPathUsesPascalCaseAsync(string pathWithPascalCaseSegment)
        {
            // Arrange
            var parameterExpression = Expression.Parameter(typeof(Foo));
            var scope = new ExpressionScope("a", parameterExpression);
            var pathProvider = new StaticProvider<Data<string>>(pathWithPascalCaseSegment);
            var sut = new ObjectPathLookupExpressionProvider<long>(pathProvider);

            // Act
            Func<Task> action = async () => await sut.Invoke(new ProviderContext(MockAutomationData.CreateWithEventTrigger()), scope);

            // Assert
            (await action.Should().ThrowAsync<ErrorException>())
                .And.Error.Code.Should().Be("automation.providers.path.syntax.error");
        }

        [Theory]
        [InlineData("#/a/wrong")]
        [InlineData("#/a/simple/y")]
        [InlineData("#/a/compoundpropertyname/y")]
        [InlineData("#/a/compoundPropertyname/y")]
        [InlineData("/policy/createdDateTime")]
        public async Task Resolve_ThrowsErrorException_WhenPathNotFoundAsync(string path)
        {
            // Arrange
            var parameterExpression = Expression.Parameter(typeof(Foo));
            var scope = new ExpressionScope("a", parameterExpression);
            var pathProvider = new StaticProvider<Data<string>>(path);
            var sut = new ObjectPathLookupExpressionProvider<long>(pathProvider);

            // Act
            Func<Task> func = async () => await sut.Invoke(new ProviderContext(MockAutomationData.CreateWithEventTrigger()), scope);

            // Assert
            (await func.Should().ThrowAsync<ErrorException>())
                .And.Error.Code.Should().Be("automation.providers.path.not.found");
        }

        [Fact]
        public async Task Resolve_ShouldResolveDefaultValue_WhenDefaultValueConfiguredAndPathNotFound()
        {
            // Arrange
            var expectedValue = 99;
            var parameterExpression = Expression.Parameter(typeof(Foo));
            var scope = new ExpressionScope("a", parameterExpression);
            var pathProvider = new StaticProvider<Data<string>>("#/a/wrong");
            var defaultValueProvider = new StaticProvider<Data<long>>(new Data<long>(expectedValue));
            var sut = new ObjectPathLookupExpressionProvider<long>(pathProvider, defaultValueProvider);

            // Act
            var expression = await sut.Invoke(new ProviderContext(MockAutomationData.CreateWithEventTrigger()), scope);

            // Assert
            var value = (ConstantExpression)expression;
            value.Value.Should().Be(expectedValue.ToString());
        }

        [Theory]
        [InlineData("{\r\n\"objectPathLookupText\": {\r\n\"path\": \"#/samplePath/propertyName\",\r\n\"valueIfNotFound\": \"default\"\r\n  }\r\n }")]
        [InlineData(@"{ ""objectPathLookupText"": {  ""path"": ""#/samplePath/propertyName"", ""valueIfNotFound"": {  ""liquidText"": { ""liquidTemplate"": ""default"",  ""dataObject"": [ {""propertyName"": ""prop1"", ""value"": ""none"" }] } } }}")]
        public void Deserialize_ShouldDeserializeConfigWithDefaultValueProvider(string config)
        {
            // Arrange + Act
            var buildModel = JsonConvert.DeserializeObject<IBuilder<IExpressionProvider>>(
                config, AutomationDeserializationConfiguration.ModelSettings);

            // Assert
            buildModel.Should().NotBeNull();
            var pathLookupModel = (ObjectPathLookupExpressionProviderConfigModel<string>)buildModel;
            pathLookupModel.Path.Should().NotBeNull();
            pathLookupModel.ValueIfNotFound.Should().NotBeNull();

            var provider = buildModel.Build(MockAutomationData.GetDefaultServiceProvider());
            provider.Should().NotBeNull();
        }

        [Fact]
        public void Deserialize_ShouldDeserializeConfigWithDataObjectProvider()
        {
            // Arrange
            var config = @"{
                ""objectPathLookupInteger"": {
                    ""path"": ""#/samplePath/propertyName"",
                    ""dataObject"": [
                        {
                            ""propertyName"": ""propertyName"",
                            ""value"": 235
                        }]}}";

            // Act
            var buildModel = JsonConvert.DeserializeObject<IBuilder<IExpressionProvider>>(
                config, AutomationDeserializationConfiguration.ModelSettings);

            // Assert
            buildModel.Should().NotBeNull();
            var pathLookupModel = (ObjectPathLookupExpressionProviderConfigModel<long>)buildModel;
            pathLookupModel.Path.Should().NotBeNull();
            pathLookupModel.DataObject.Should().NotBeNull();

            var provider = buildModel.Build(MockAutomationData.GetDefaultServiceProvider());
            provider.Should().NotBeNull();
        }

        private class Bar
        {
            public long X { get; set; }
        }

        private class Foo
        {
            public Bar Simple { get; set; }

            public Bar CompoundPropertyName { get; set; }
        }
    }
}
