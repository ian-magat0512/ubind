// <copyright file="MsExcelFileProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.File
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.File;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.FileHandling;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using Xunit;

    /// <summary>
    /// Unit tests for <see cref="MsExcelFileProvider"/>.
    /// </summary>
    public class MsExcelFileProviderTests
    {
        private const string SourceFilename = "Test Report Template.xlsx";
        private readonly string jsonString;

        public MsExcelFileProviderTests()
        {
            this.jsonString = @"{
    ""quotes"": [
        {
            ""createdDate"": ""2020-01-01"",
            ""quoteReference"": ""JSDEBN"",
            ""policyTransactionType"": ""newBusiness"",
            ""customer"": {
                ""displayName"": ""Peter Smith""
            },
            ""calculation"": {
                ""payment"": {
                    ""payableComponents"": {
                        ""totalPayable"": ""$1,231""
                    }
                }
            },
            ""questions"": {
                ""ratingState"": ""VIC""
            }
        },
    ],
    ""events"": [
        {
            ""createdDate"": ""2020-02-01"",
            ""createdTime"": ""21:57:39+00:00"",
            ""eventType"": ""quoteActualised"",
            ""tags"": [
                ""My tag name"",
                ""Another tag"",
                ""One last tag""
            ]
        },
    ]
}";
        }

        [Fact]
        public async Task MsExcelFileProvider_ShouldSucceed_RenderingDataObjectFromAutomationData()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger(withTriggerContent: true);
            var sourceFileProvider = new StaticProvider<Data<FileInfo>>(new FileInfo(SourceFilename, null));
            var fileProvider = this.CreateMsExcelFileProvider(sourceFileProvider);

            // Act
            var fileInfo = await fileProvider.Resolve(new ProviderContext(automationData));

            // Assert
            fileInfo.GetValueOrThrowIfFailed().DataValue.Should().NotBeNull();
        }

        [Fact]
        public async Task MsExcelFileProvider_ShouldSucceed_RenderingDataObjectFromObjectProvider()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var objectProvider = this.BuildJsonObjectProvider(this.jsonString);
            var sourceFileProvider = new StaticProvider<Data<FileInfo>>(new FileInfo(SourceFilename, null));
            var fileProvider = this.CreateMsExcelFileProvider(
                objectProvider: objectProvider,
                sourceFileProvider: sourceFileProvider);

            // Act
            var fileInfo = await fileProvider.Resolve(new ProviderContext(automationData));

            // Assert
            fileInfo.GetValueOrThrowIfFailed().DataValue.Should().NotBeNull();
        }

        [Fact]
        public async Task MsExcelFileProvider_ShouldReturnDevelopment_OnEnvironmentGet()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var objectProvider = this.BuildJsonObjectProvider(this.jsonString);
            var sourceFileProvider = new StaticProvider<Data<FileInfo>>(new FileInfo(SourceFilename, null));
            var fileProvider = this.CreateMsExcelFileProvider(
                objectProvider: objectProvider,
                sourceFileProvider: sourceFileProvider);

            // Act
            await fileProvider.Resolve(new ProviderContext(automationData));

            // Assert
            fileProvider.Environment.Should().Be(DeploymentEnvironment.Development);
        }

        [Fact]
        public async Task MsExcelFileProvider_ShouldReturnEntity_OnGetEntity()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var objectProvider = this.BuildJsonObjectProvider(this.jsonString);
            var sourceFileProvider = new StaticProvider<Data<FileInfo>>(new FileInfo(SourceFilename, null));
            var fileProvider = this.CreateMsExcelFileProvider(
                objectProvider: objectProvider,
                sourceFileProvider: sourceFileProvider);

            // Act
            await fileProvider.Resolve(new ProviderContext(automationData));
            var entity = fileProvider.GetObject("quotes");

            // Assert
            entity.HasValues.Should().BeTrue();
        }

        [Theory]
        [InlineData("Test Report Template.txt", "test.xlsx", "automation.excel.provider.source.invalid", "Invalid source filename for an excel file.")]
        [InlineData("Test Report Template.xlsx", "test*.xlsx", "file.name.invalid", "File must have a valid filename.")]
        [InlineData("Test Report Template.xlsx", "test.txt", "automation.excel.provider.output.invalid", "Invalid output filename for an excel file.")]
        public async Task MsExcelFileProvider_ShouldRaiseErrors_WhenFileValidationFails(
            string sourceFileName, string outputFileName, string errorCode, string errorTitle)
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var sourceFileProvider = new StaticProvider<Data<FileInfo>>(new FileInfo(sourceFileName, null));
            var outputFileNameProvider = new StaticProvider<Data<string>>(outputFileName);
            var msExcelProvider = this.CreateMsExcelFileProvider(sourceFileProvider, outputFileNameProvider);

            // Act
            Func<Task> func = async () => await msExcelProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be(errorCode);
            exception.Which.Error.Title.Should().Be(errorTitle);
        }

        private MsExcelFileProvider CreateMsExcelFileProvider(
            IProvider<Data<FileInfo>> sourceFileProvider,
            IProvider<Data<string>> outputFileNameProvider = null,
            IObjectProvider objectProvider = null)
        {
            var engine = new Mock<IMsExcelEngineService>();
            var provider = new MsExcelFileProvider(
                outputFileNameProvider,
                sourceFileProvider,
                objectProvider,
                engine.Object);
            return provider;
        }

        private IObjectProvider BuildJsonObjectProvider(string jsonString)
        {
            var textProviderModel = new StaticBuilder<Data<string>>() { Value = jsonString };
            var providerModel = new JsonTextToObjectProviderConfigModel() { TextProvider = textProviderModel };
            var objectProvider = providerModel.Build(null);
            return objectProvider;
        }
    }
}
