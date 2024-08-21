// <copyright file="GemBoxMsWordEngineServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.FileHandling
{
    using System.Text;
    using CSharpFunctionalExtensions;
    using FluentAssertions;
    using GemBox.Document;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Providers.File;
    using UBind.Application.FileHandling.GemBoxServices;
    using UBind.Application.Queries.FileAttachment;
    using UBind.Application.Services.Email;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using Xunit;

    /// <summary>
    /// Unit tests for the <see cref="GemBoxMsWordEngineServiceTests"/>.
    /// </summary>
    public class GemBoxMsWordEngineServiceTests
    {
        private readonly GemBoxMsWordEngineService service;
        private readonly ILoggerFactory loggerFactory;

        public GemBoxMsWordEngineServiceTests()
        {
            this.loggerFactory = LoggerFactory.Create(builder => { });
            var logger = this.loggerFactory.CreateLogger<GemBoxMsWordEngineService>();
            var cqrsLogger = this.loggerFactory.CreateLogger<CqrsMediator>();
            var mediator = new CqrsMediator(new Mock<IServiceProvider>().Object, cqrsLogger, new Mock<IErrorNotificationService>().Object);
            this.service = new GemBoxMsWordEngineService(logger, mediator);
            ComponentInfo.SetLicense("FREE-LIMITED-KEY");
            ComponentInfo.FreeLimitReached += (sender, e) => e.FreeLimitReachedAction = FreeLimitReachedAction.ContinueAsTrial;
        }

        private enum MergeFieldType
        {
            Text,
            Html,
            Image,
            Document,
        }

        [Theory]
        [InlineData("<h1>A valid HTML string</h1>")]
        [InlineData("Another valid<div> HTML string</h1>")]
        [InlineData("<html><head></head><body><div>Yet<p> another valid HTML string</p></div></body></html>")]
        public void MergeDataToTemplate_ShouldSucceed_WhenTheValueForAnHTMLFieldIsAValidHTMLString(string htmlString)
        {
            // Arrange
            var expectedErrorCode = "merge.field.value.html.data.type.invalid";
            var dataSource = JObject.FromObject(
                new
                {
                    HtmlString = htmlString,
                });

            var document = this.CreateDocumentWithMergeFields(new TestMergeFields("HtmlString", MergeFieldType.Html));
            var templateContent = this.ExtractDocumentContent(document);

            // Act
            Func<byte[]> func = () => this.service.MergeDataToTemplate(
                Guid.NewGuid(), dataSource, templateContent, false, false, false, false, false, new List<ContentSourceFile>());
            var outputContent = func();

            // Assert
            func.Should().NotThrow();
            outputContent.Length.Should().BeGreaterThan(templateContent.Length);
        }

        [Fact]
        public void MergeDataToTemplate_ShouldRaiseAnError_WhenTheValueForAnHTMLFieldIsNotAString()
        {
            // Arrange
            var expectedErrorCode = "merge.field.value.html.data.type.invalid";
            var dataSource = JObject.FromObject(
                new
                {
                    details = "<h1>Business Details</h1>",
                    information = 12345,
                });

            var type = MergeFieldType.Html;
            var document = this.CreateDocumentWithMergeFields(
                new TestMergeFields("details", type),
                new TestMergeFields("information", type));
            var templateContent = this.ExtractDocumentContent(document);

            // Act
            Func<byte[]> func = () => this.service.MergeDataToTemplate(
                Guid.NewGuid(), dataSource, templateContent, false, false, false, false, false, new List<ContentSourceFile>());

            // Assert
            func.Should().Throw<ErrorException>().Which.Error.Code.Should().Be(expectedErrorCode);
        }

        [Fact]
        public void MergeDataToTemplate_ShouldRaiseAnError_WhenTheValueForAnHTMLFieldIsNotFormattedCorrectly()
        {
            // Arrange
            var expectedErrorCode = "merge.field.value.html.content.invalid";
            var dataSource = JObject.FromObject(
                new
                {
                    details = "<h1>Business Details</h1>",
                    information =
                        "<span>This value does not contain any valid HTML block element such as div, p, h1, table, etc.</span>",
                });

            var type = MergeFieldType.Html;
            var document = this.CreateDocumentWithMergeFields(
                new TestMergeFields("details", type),
                new TestMergeFields("information", type));
            var templateContent = this.ExtractDocumentContent(document);

            // Act
            Func<byte[]> func = () => this.service.MergeDataToTemplate(
                Guid.NewGuid(), dataSource, templateContent, true, true, true, true, true, new List<ContentSourceFile>());

            // Assert
            func.Should().Throw<ErrorException>().Which.Error.Code.Should().Be(expectedErrorCode);
        }

        [Fact]
        public void MergeDataToTemplate_ShouldRaiseAnError_WhenTheAttachmentForAnImageFieldIsNotFound()
        {
            // Arrange
            var logger = this.loggerFactory.CreateLogger<GemBoxMsWordEngineService>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            Maybe<IFileContentReadModel> fileReadModel = null;
            var mockQuoteAttachmentRepository = new Mock<IFileAttachmentRepository<QuoteFileAttachment>>();
            var mockClaimAttachmentRepository = new Mock<IFileAttachmentRepository<ClaimFileAttachment>>();
            mockServiceProvider.Setup(a => a.GetService(typeof(IFileAttachmentRepository<QuoteFileAttachment>)))
                .Returns(mockQuoteAttachmentRepository.Object);
            mockServiceProvider.Setup(a => a.GetService(typeof(IFileAttachmentRepository<ClaimFileAttachment>)))
                .Returns(mockClaimAttachmentRepository.Object);
            var fileAttachmentQuery = new GetFileAttachmentContentQueryHandler(
                mockQuoteAttachmentRepository.Object,
                mockClaimAttachmentRepository.Object);
            mockServiceProvider.Setup(a => a.GetService(typeof(GetFileAttachmentContentQueryHandler)))
                .Returns(fileAttachmentQuery);
            mockServiceProvider.Setup(a => a.GetService(typeof(IMediator)))
                .Returns(new Mock<IMediator>().Object);
            var mockErrorNotificationService = new Mock<IErrorNotificationService>();
            var cqrsLogger = this.loggerFactory.CreateLogger<CqrsMediator>();
            var mockMediator = new Mock<CqrsMediator>(
                mockServiceProvider.Object,
                cqrsLogger,
                new Mock<IErrorNotificationService>().Object);
            var mediator = mockMediator.Object;
            var sut = new GemBoxMsWordEngineService(logger, mediator);
            var expectedErrorCode = "merge.field.value.image.file.attachment.not.found";
            var dataSource = JObject.FromObject(
                new
                {
                    TestImage = "Test Image Four.jpg:image/jpeg:DF5147EF-5EAB-4911-B86D-46FD0EAB82EF:1280:831:169845",
                });

            var type = MergeFieldType.Image;
            var document = this.CreateDocumentWithMergeFields(
                new TestMergeFields("TestImage", type));
            var templateContent = this.ExtractDocumentContent(document);

            // Act
            Func<byte[]> func = () => sut.MergeDataToTemplate(
                Guid.NewGuid(), dataSource, templateContent, true, true, true, true, true, new List<ContentSourceFile>());

            // Assert
            func.Should().Throw<ErrorException>().Which.Error.Code.Should().Be(expectedErrorCode);
        }

        [Fact]
        public void MergeDataToTemplate_ShouldRaiseAnError_WhenTheValueForAnImageFieldIsValidButTheReferencedAttachmentIsNotAnImage()
        {
            // Arrange
            var logger = this.loggerFactory.CreateLogger<GemBoxMsWordEngineService>();

            // content of a text file
            var textFileContent = "0x46656174757265205541543A200D0A68747470733A2F2F6275676669782D75622D3"
                + "1303034362E666561747572652D7561742E7562696E642E696F2F706F7274616C2F7562696E64";
            var fileReadModel = new FileContentReadModel();
            fileReadModel.FileContent = Encoding.UTF8.GetBytes(textFileContent);
            var mockMediator = new Mock<ICqrsMediator>();
            mockMediator.Setup(a => a.Send(
                It.IsAny<GetFileAttachmentContentQuery>(),
                It.IsAny<CancellationToken>()).Result)
                .Returns(fileReadModel);
            var mediator = mockMediator.Object;
            var sut = new GemBoxMsWordEngineService(logger, mediator);
            var expectedErrorCode = "merge.field.value.image.file.attachment.invalid";
            var dataSource = JObject.FromObject(
                new
                {
                    TestImage = "Test Image Four.jpg:image/jpeg:DF5147EF-5EAB-4911-B86D-46FD0EAB82EF:1280:831:169845",
                });

            var type = MergeFieldType.Image;
            var document = this.CreateDocumentWithMergeFields(
                new TestMergeFields("TestImage", type));
            var templateContent = this.ExtractDocumentContent(document);

            // Act
            Func<byte[]> func = () => sut.MergeDataToTemplate(
                Guid.NewGuid(), dataSource, templateContent, true, true, true, true, true, new List<ContentSourceFile>());

            // Assert
            func.Should().Throw<ErrorException>().Which.Error.Code.Should().Be(expectedErrorCode);
        }

        [Fact]
        public void MergeDataToTemplate_ShouldSucceed_WhenTheValueForAnImageFieldIsValidAndTheReferencedAttachmentIsAnImage()
        {
            // Arrange
            var logger = this.loggerFactory.CreateLogger<GemBoxMsWordEngineService>();

            // content of an image file
            var textFileContent = "iVBORw0KGgoAAAANSUhEUgAAAH8AAABeCAIAAACFL1zXAAAAAXNSR0IArs4c6QAA"
                + "AARnQU1BAACxjwv8YQUAAAAJcEhZcwAAHYcAAB2HAY/l8WUAAAGtSURBVHhe7dfBicJAHEbx7WM7sIPYgQ0E+"
                + "7AAwbuIdcQCBDvI3Q5swAqyH04MgoIuObzs8n6n+U+Ch5cw6lcnjvVJ1idZn2R9kvVJ1idZn2R9kvVJ1idZn2R9"
                + "kvVJ1idZn2R9kvVJ1idZn2R9kvVJ1idZn2R9kvVJ1idZn2R9kvVJ1idZn2R9kvVJ1idZn2R9kvVJ1idZn2R9kvVJk"
                + "67//WSxWGR/u932803GbJ7P57quM85ms81m07ZtufpovV7fPngq/sC7Xzr2w01yL5fLfrirqmq/32dxuVxOp1PZjJc3"
                + "T8T/qZ97HqMPrD/Kh/VXq1XOnKZp+vnO+qO8rJ+dQb/bdbvdLg8gjsdjv2X9kT589wd5Brk/X8JltP4ov60f8/n8cDiU"
                + "tfVH+aR++Z1zvV6zztGf+7NTLll/lLfnfsa0zl+BnPgZs/Dc13vWJ1mfZH2S9UnWJ1mfZH2S9UnWJ1mfZH2S9UnWJ1mf"
                + "ZH2S9UnWJ1mfZH2S9UnWJ1mfZH2S9UnWJ1mfZH2S9UnWJ1mfZH2S9UnWJ1mfZH2S9UnWJ1mfZH2S9UnWJ1mf03U/4LmZ"
                + "i6m2sfUAAAAASUVORK5CYII=";

            var fileReadModel = new FileContentReadModel();
            fileReadModel.FileContent = Convert.FromBase64String(textFileContent);
            var mockMediator = new Mock<ICqrsMediator>();
            mockMediator.Setup(a => a.Send(
                It.IsAny<GetFileAttachmentContentQuery>(),
                It.IsAny<CancellationToken>()).Result)
                .Returns(fileReadModel);
            var mediator = mockMediator.Object;
            var sut = new GemBoxMsWordEngineService(logger, mediator);
            var expectedErrorCode = "invalid.image.attachment";
            var dataSource = JObject.FromObject(
                new
                {
                    TestImage = "Test Image Four.jpg:image/jpeg:DF5147EF-5EAB-4911-B86D-46FD0EAB82EF:1280:831:169845",
                });

            var type = MergeFieldType.Image;
            var document = this.CreateDocumentWithMergeFields(
                new TestMergeFields("TestImage", type));
            var templateContent = this.ExtractDocumentContent(document);

            // Act
            Func<byte[]> func = () => sut.MergeDataToTemplate(
                Guid.NewGuid(), dataSource, templateContent, true, true, true, true, true, new List<ContentSourceFile>());
            var mergedContent = func();

            // Assert
            func.Should().NotThrow();
            mergedContent.Length.Should().BeGreaterThan(templateContent.Length);
        }

        [Fact]
        public void MergeDataToTemplate_ShouldRaiseAnError_WhenTheValueForAnHTMLFieldContainsAnInvalidHTMLStructure()
        {
            // Arrange
            var expectedErrorCode = "merge.field.value.html.content.invalid";
            var dataSource = JObject.FromObject(
                new
                {
                    details = "<h1>Business Details</h1>",
                    information =
                        "<tr>Using tr without table is an invalid HTML structure.</tr>",
                });

            var type = MergeFieldType.Html;
            var document = this.CreateDocumentWithMergeFields(
                new TestMergeFields("details", type),
                new TestMergeFields("information", type));
            var templateContent = this.ExtractDocumentContent(document);

            // Act
            Func<byte[]> func = () => this.service.MergeDataToTemplate(
                Guid.NewGuid(), dataSource, templateContent, true, true, true, true, true, new List<ContentSourceFile>());

            // Assert
            func.Should().Throw<ErrorException>().Which.Error.Code.Should().Be(expectedErrorCode);
        }

        [Fact]
        public void MergeDataToTemplate_ShouldRaiseAnError_WhenTheValueForAMergeFieldIsNull()
        {
            // Arrange
            var expectedErrorCode = "merge.field.value.null";
            var dataSource = JObject.FromObject(
                new
                {
                    details = JValue.CreateNull(),
                    information =
                        "<tr>Using tr without table is an invalid HTML structure.</tr>",
                });

            var type = MergeFieldType.Html;
            var document = this.CreateDocumentWithMergeFields(
                new TestMergeFields("details", type),
                new TestMergeFields("information", type));
            var templateContent = this.ExtractDocumentContent(document);

            // Act
            Func<byte[]> func = () => this.service.MergeDataToTemplate(
                Guid.NewGuid(), dataSource, templateContent, true, true, true, true, true, new List<ContentSourceFile>());

            // Assert
            func.Should().Throw<ErrorException>().Which.Error.Code.Should().Be(expectedErrorCode);
        }

        [Fact]
        public void MergeDataToTemplate_ShouldRaiseAnError_WhenTheSourceFileForAWordContentMergeFieldIsAWordDocument()
        {
            // Arrange
            var expectedErrorCode = "merge.field.value.word.content.file.format.invalid";
            var contentSourceFiles = new List<ContentSourceFile>();
            contentSourceFiles.Add(
                new ContentSourceFile(
                    new FileInfo("invalid.png", new byte[100]),
                    "details",
                    true));
            var type = MergeFieldType.Document;
            var document = this.CreateDocumentWithMergeFields(
                new TestMergeFields("Details", type));
            var templateContent = this.ExtractDocumentContent(document);

            // Act
            Func<byte[]> func = () => this.service.MergeDataToTemplate(
                Guid.NewGuid(), new JObject(), templateContent, true, true, true, true, true, contentSourceFiles);

            // Assert
            func.Should().Throw<ErrorException>().Which.Error.Code.Should().Be(expectedErrorCode);
        }

        private DocumentModel CreateDocumentWithMergeFields(params TestMergeFields[] fields)
        {
            var document = new DocumentModel();
            List<Paragraph> blocks = new List<Paragraph>();
            foreach (var field in fields)
            {
                var fieldType = "Text";
                switch (field.FieldType)
                {
                    case MergeFieldType.Html:
                        fieldType = "Html";
                        break;
                    case MergeFieldType.Image:
                        fieldType = "Image";
                        break;
                    case MergeFieldType.Document:
                        fieldType = "WordContent";
                        break;
                }

                blocks.Add(
                    new Paragraph(
                        document,
                        new Field(document, FieldType.MergeField, $"{fieldType}:{field.FieldName}")));
            }

            document.Sections.Add(new Section(document, blocks));
            return document;
        }

        private byte[] ExtractDocumentContent(DocumentModel document)
        {
            using (var stream = new MemoryStream())
            {
                document.Save(stream, SaveOptions.DocxDefault);
                return stream.ToArray();
            }
        }

        private class TestMergeFields
        {
            public TestMergeFields(string fieldName, MergeFieldType fieldType)
            {
                this.FieldName = fieldName;
                this.FieldType = fieldType;
            }

            public string FieldName { get; private set; }

            public MergeFieldType FieldType { get; private set; }
        }
    }
}
