// <copyright file="MsWordFileProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.File
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.File;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.FileHandling.GemBoxServices;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using Xunit;

    /// <summary>
    /// Unit tests for <see cref="MSWordFileProvider"/>.
    /// </summary>
    public class MsWordFileProviderTests
    {
        private ILoggerFactory loggerFactory;
        private string testFolder;

        public MsWordFileProviderTests()
        {
            this.loggerFactory = LoggerFactory.Create(builder => { });
            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().Location);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            this.testFolder = Path.GetDirectoryName(codeBasePath);
        }

        [Fact]
        public async Task MsWordFileProvider_ShouldSucceed_RenderingDataObjectFromAutomationData()
        {
            // Arrange
            var expectedOutputFile = "document-quote.doc";
            var automationData = await MockAutomationData.CreateWithHttpTrigger(withTriggerContent: true);
            var filePath = Path.Combine(this.testFolder, "Templates\\Documents\\document-policy-schedule.dotx");
            byte[] templateContent = System.IO.File.ReadAllBytes(filePath);
            var sourceFileProvider = new BinaryFileProvider(
                new StaticProvider<Data<string>>(expectedOutputFile),
                new StaticProvider<Data<byte[]>>(templateContent));

            var engine = new Mock<IGemBoxMsWordEngineService>();

            var fileProvider = this.CreateMsWordFileProvider(engine.Object, sourceFileProvider);

            engine.Setup(
                we => we.MergeDataToTemplate(
                    It.IsAny<Guid>(),
                    It.IsAny<JObject>(),
                    It.IsAny<byte[]>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<IEnumerable<ContentSourceFile>>()))
                .Returns(templateContent);

            // Act
            FileInfo fileInfo = (await fileProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            fileInfo.FileName.ToString().Should().Be(expectedOutputFile);
            fileInfo.Content.Length.Should().Be(templateContent.Length);
        }

        [Fact]
        public async Task MsWordFileProvider_ShouldSucceed_RenderingDataObjectFromObjectProvider()
        {
            // Arrange
            var expectedOutputFile = "document-quote.doc";
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var dummyData = new Dictionary<IProvider<Data<string>>, IProvider<IData>>()
            {
                { new StaticProvider<Data<string>>("ContactName"), (IProvider<IData>)new StaticProvider<Data<string>>("Jane Doe") },
                { new StaticProvider<Data<string>>("ContactAddress"), (IProvider<IData>)new StaticProvider<Data<long>>(100) },
                { new StaticProvider<Data<string>>("ContactTown"), (IProvider<IData>)new StaticProvider<Data<bool>>(false) },
            };
            var objectProvider = new DynamicObjectProvider(
                dummyData,
                new Mock<ILogger<DynamicObjectProvider>>().Object,
                automationData.ServiceProvider);

            var filePath = Path.Combine(this.testFolder, "Templates\\Documents\\document-quote-schedule.dotx");
            byte[] templateContent = System.IO.File.ReadAllBytes(filePath);
            var sourceFileProvider = new BinaryFileProvider(
                new StaticProvider<Data<string>>(expectedOutputFile),
                new StaticProvider<Data<byte[]>>(templateContent));

            var engine = new Mock<IGemBoxMsWordEngineService>();

            var fileProvider = this.CreateMsWordFileProvider(
                engine.Object,
                dataObjectProvider: objectProvider,
                sourceFileProvider: sourceFileProvider);

            engine.Setup(
                we => we.MergeDataToTemplate(
                    It.IsAny<Guid>(),
                    It.IsAny<JObject>(),
                    It.IsAny<byte[]>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>(),
                    It.IsAny<IEnumerable<ContentSourceFile>>()))
                .Returns(templateContent);

            // Act
            FileInfo fileInfo = (await fileProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            fileInfo.FileName.ToString().Should().Be(expectedOutputFile);
            fileInfo.Content.Length.Should().Be(templateContent.Length);
        }

        [Theory]
        [InlineData("data.txt", "data.doc", "inputfile.extension.not.supported")]
        [InlineData("data.doc", "data*.docx", "file.name.invalid")]
        [InlineData("data.doc", "data.txt", "outputfile.extension.not.supported")]
        public async Task MsWordFileProvider_ShouldRaiseErrors_IfDataRetrievedFailsValidation(
            string sourceFileName,
            string outputFileName,
            string expectedErrorCode)
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var filePath = Path.Combine(this.testFolder, "Templates\\Documents\\document-policy-schedule.dotx");
            byte[] templateContent = System.IO.File.ReadAllBytes(filePath);
            var sourceFileProvider = new BinaryFileProvider(
                new StaticProvider<Data<string>>(sourceFileName),
                new StaticProvider<Data<byte[]>>(templateContent));
            var outputFileNameProvider = new StaticProvider<Data<string>>(outputFileName);
            var engineService = new Mock<IGemBoxMsWordEngineService>().Object;

            var msWordProvider = this.CreateMsWordFileProvider(engineService, sourceFileProvider, outputFileNameProvider);

            // Act
            Func<Task> func = async () => await msWordProvider.Resolve(new ProviderContext(automationData));

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be(expectedErrorCode);
        }

        private MSWordFileProvider CreateMsWordFileProvider(
            IGemBoxMsWordEngineService gemBoxEngineService,
            IProvider<Data<FileInfo>> sourceFileProvider,
            IProvider<Data<string>> outputFileNameProvider = null,
            IObjectProvider dataObjectProvider = null,
            IProvider<Data<bool>> flattenDataObjectProvider = null,
            IProvider<Data<long>> repeatingRangeStartIndexProvider = null,
            IEnumerable<ContentSourceFileProvider> contentProviders = null,
            IProvider<Data<bool>> removeUnusedMergeFieldsProvider = null,
            IProvider<Data<bool>> removeRangesWhereAllMergeFieldsAreUnusedProvider = null,
            IProvider<Data<bool>> removeTablesWhereAllMergeFieldsAreUnusedProvider = null,
            IProvider<Data<bool>> removeTableRowsWhereAllMergeFieldsAreUnusedProvider = null,
            IProvider<Data<bool>> removeParagraphsWhereAllMergeFieldsAreUnusedProvider = null)
        {
            var provider = new MSWordFileProvider(
                outputFileNameProvider,
                sourceFileProvider,
                dataObjectProvider,
                flattenDataObjectProvider,
                repeatingRangeStartIndexProvider,
                contentProviders,
                removeUnusedMergeFieldsProvider,
                removeRangesWhereAllMergeFieldsAreUnusedProvider,
                removeTablesWhereAllMergeFieldsAreUnusedProvider,
                removeTableRowsWhereAllMergeFieldsAreUnusedProvider,
                removeParagraphsWhereAllMergeFieldsAreUnusedProvider,
                gemBoxEngineService,
                this.loggerFactory.CreateLogger<MSWordFileProvider>());
            return provider;
        }
    }
}
