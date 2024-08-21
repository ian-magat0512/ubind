// <copyright file="PdfFileProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.File
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using MorseCode.ITask;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.File;
    using UBind.Application.FileHandling;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ValueTypes;
    using Xunit;

    /// <summary>
    /// Unit tests for <see cref="PdfFileProvider"/>.
    /// </summary>
    public class PdfFileProviderTests
    {
        private readonly Mock<IProvider<Data<FileInfo>>> sourceMock;
        private readonly Mock<IProvider<Data<string>>> outputMock;
        private readonly Mock<IPdfEngineService> pdfEngineServiceMock;

        public PdfFileProviderTests()
        {
            this.sourceMock = new Mock<IProvider<Data<FileInfo>>>(MockBehavior.Strict);
            this.outputMock = new Mock<IProvider<Data<string>>>(MockBehavior.Strict);
            this.pdfEngineServiceMock = new Mock<IPdfEngineService>(MockBehavior.Strict);
        }

        [Theory]
        [InlineData("output.txt", "supported.doc", "outputfile.extension.not.supported")]
        public async Task Resolve_ShouldRaiseError_ValidationFails(
            string outputFileName, string sourceFileName, string expectedCode)
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var sourceFileForMock = new Data<FileInfo>(new FileInfo(sourceFileName, null));
            var outputFileForMock = new Data<string>(outputFileName);

            this.sourceMock.Setup(s => s.Resolve(It.IsAny<ProviderContext>())).Returns(
                Task.FromResult(ProviderResult<Data<FileInfo>>.Success(sourceFileForMock)).AsITask());
            this.outputMock.Setup(s => s.Resolve(It.IsAny<ProviderContext>())).Returns(
                Task.FromResult(ProviderResult<Data<string>>.Success(outputFileForMock)).AsITask());

            var pdfFileProvider = this.CreatePdfFileProvider(
                this.outputMock.Object,
                this.sourceMock.Object,
                this.pdfEngineServiceMock.Object);

            // Act
            Func<Task> func = async () => await pdfFileProvider.Resolve(new ProviderContext(automationData));

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be(expectedCode);
        }

        [Fact]
        public async Task Resolve_ShouldSucceed_ConvertMsDocToPdf()
        {
            // Arrange
            var expectedOutputFile = "document-policy-schedule.pdf";
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var testDocumentMock = "This is a string which will be converted to array of bytes for mocking purposes";
            var templateContent = Encoding.ASCII.GetBytes(testDocumentMock);

            var sourceMock = new Mock<IProvider<Data<FileInfo>>>(MockBehavior.Strict);
            var outputMock = new Mock<IProvider<Data<string>>>(MockBehavior.Strict);

            var sourceFileForMock = new Data<FileInfo>(new FileInfo("document-policy-schedule.docx", templateContent));
            var outputFileForMock = new Data<string>(expectedOutputFile);

            sourceMock.Setup(sm => sm.Resolve(It.IsAny<ProviderContext>())).Returns(
                Task.FromResult(ProviderResult<Data<FileInfo>>.Success(sourceFileForMock)).AsITask());
            outputMock.Setup(om => om.Resolve(It.IsAny<ProviderContext>())).Returns(
                Task.FromResult(ProviderResult<Data<string>>.Success(outputFileForMock)).AsITask());
            this.pdfEngineServiceMock.Setup(pes => pes.OutputSourceFileBytesToPdfBytes(
                It.IsAny<FileInfo>(), It.IsAny<JObject>())).
                 Returns(templateContent);

            var pdfFileProvider = this.CreatePdfFileProvider(
                outputMock.Object,
                sourceMock.Object,
                this.pdfEngineServiceMock.Object);

            // Act
            var result = (await pdfFileProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            result.DataValue.FileName.ToString().Should().Be(expectedOutputFile);
            result.DataValue.Content.Length.Should().Be(templateContent.Length);
        }

        [Fact]
        public async Task Resolve_ShouldRaiseException_WhenEngineReturnsEmptyByteLengthOrNull()
        {
            // Assign
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var testDocumentMock = string.Empty;
            var templateContent = Encoding.ASCII.GetBytes(testDocumentMock);

            var sourceMock = new Mock<IProvider<Data<FileInfo>>>(MockBehavior.Strict);
            var outputMock = new Mock<IProvider<Data<string>>>(MockBehavior.Strict);
            var sourceFileNameMock = "document-policy-schedule.docx";
            var fileName = new FileName("document-policy-schedule-temp.docx");
            var sourceFileForMock = new Data<FileInfo>(new FileInfo(sourceFileNameMock, templateContent));
            var outputFileForMock = new Data<string>("document-policy-schedule.pdf");

            sourceMock.Setup(sm => sm.Resolve(It.IsAny<ProviderContext>())).Returns(
                Task.FromResult(ProviderResult<Data<FileInfo>>.Success(sourceFileForMock)).AsITask());
            outputMock.Setup(om => om.Resolve(It.IsAny<ProviderContext>())).Returns(
                Task.FromResult(ProviderResult<Data<string>>.Success(outputFileForMock)).AsITask());
            this.pdfEngineServiceMock.Setup(pes => pes.OutputSourceFileBytesToPdfBytes(
                It.IsAny<FileInfo>(), It.IsAny<JObject>())).
                Returns(templateContent);

            var pdfFileProvider = this.CreatePdfFileProvider(
               outputMock.Object,
               sourceMock.Object,
               this.pdfEngineServiceMock.Object);

            // Act
            Func<Task> func = async () => await pdfFileProvider.Resolve(new ProviderContext(automationData));

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("pdf.file.empty");
        }

        [Fact]
        public async Task Resolve_ShouldRaiseException_WhenEngineReturnsNullOutput()
        {
            // Assign
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var testDocumentMock = string.Empty;
            byte[] templateContent = Encoding.ASCII.GetBytes(testDocumentMock);

            var sourceMock = new Mock<IProvider<Data<FileInfo>>>(MockBehavior.Strict);
            var outputMock = new Mock<IProvider<Data<string>>>(MockBehavior.Strict);
            var sourceFileNameMock = "document-policy-schedule.docx";
            var outputFileMock = "document-policy-schedule.pdf";
            var sourceFileForMock = new Data<FileInfo>(new FileInfo(sourceFileNameMock, templateContent));
            var outputFileForMock = new Data<string>(outputFileMock);

            sourceMock.Setup(sm => sm.Resolve(It.IsAny<ProviderContext>())).Returns(
                Task.FromResult(ProviderResult<Data<FileInfo>>.Success(sourceFileForMock)).AsITask());
            outputMock.Setup(om => om.Resolve(It.IsAny<ProviderContext>())).Returns(
                Task.FromResult(ProviderResult<Data<string>>.Success(outputFileForMock)).AsITask());
            byte[] engineReturnsNull = null;
            this.pdfEngineServiceMock.Setup(pes => pes.OutputSourceFileBytesToPdfBytes(
                It.IsAny<FileInfo>(), It.IsAny<JObject>())).
                 Returns(engineReturnsNull);

            var pdfFileProvider = this.CreatePdfFileProvider(
               outputMock.Object,
               sourceMock.Object,
               this.pdfEngineServiceMock.Object);

            // Act
            Func<Task> func = async () => await pdfFileProvider.Resolve(new ProviderContext(automationData));

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("pdf.file.is.null");
        }

        private PdfFileProvider CreatePdfFileProvider(
            IProvider<Data<string>> outputFileNameProvider,
            IProvider<Data<FileInfo>> sourceFileProvider,
            IPdfEngineService pdfEngineService)
        {
            var pdfFileProvider = new PdfFileProvider(
                outputFileNameProvider,
                sourceFileProvider,
                pdfEngineService,
                new Mock<ILogger<PdfFileProvider>>().Object);
            return pdfFileProvider;
        }
    }
}
