// <copyright file="ApplicationQuoteFileAttachmentServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Services
{
    using System;
    using CSharpFunctionalExtensions;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class ApplicationQuoteFileAttachmentServiceTests
    {
        private readonly Mock<IFileAttachmentRepository<IFileAttachment>> fileAttachmentRepository = new Mock<IFileAttachmentRepository<IFileAttachment>>();
        private readonly Mock<IClock> clock = new Mock<IClock>();
        private readonly Mock<IQuoteAggregateResolverService> quoteAggregateResolverService = new Mock<IQuoteAggregateResolverService>();

        [Fact]
        public void GetFileAttachmentContentForQuote_ExistingFileAttachment_ShouldReturnCorrectlyEncodedFileContent()
        {
            // Arrange
            /*var dbContext = new UBindDbContext("UBindTestDatabase");*/
            var attachmentId = Guid.NewGuid();
            var content = "Hello world!";
            var filename = "hello.txt";
            var contentInBytes = System.Text.Encoding.ASCII.GetBytes(content);
            var contentInBase64 = Convert.ToBase64String(contentInBytes);
            var defaultQuoteWorkflowProvider = new DefaultQuoteWorkflowProvider();
            var quoteExpirySettingsProvider = new DefaultExpirySettingsProvider();
            var quote = QuoteAggregate.CreateNewBusinessQuote(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                DeploymentEnvironment.Development,
                QuoteExpirySettings.Default,
                default(Guid),
                this.clock.Object.Now(),
                Guid.NewGuid(),
                Timezones.AET,
                false,
                null,
                true);

            var quoteAggregate = quote.Aggregate;
            quoteAggregate.AttachFile(
                attachmentId, Guid.NewGuid(), filename, "text", contentInBytes.Length, Guid.NewGuid(), this.clock.Object.Now(), quote.Id);
            this.quoteAggregateResolverService
                .Setup(q => q.GetQuoteAggregateForQuote(quoteAggregate.TenantId, quote.Id))
                .Returns(quoteAggregate);

            var fileContentReadModel =
                new FileContentReadModel { ContentType = "text", Name = filename, FileContent = contentInBytes };
            this.fileAttachmentRepository
                .Setup(f => f.GetAttachmentContent(quoteAggregate.TenantId, It.IsAny<Guid>()))
                .Returns(Maybe<IFileContentReadModel>.From(fileContentReadModel));

            var sut =
                new ApplicationQuoteFileAttachmentService(
                    this.fileAttachmentRepository.Object,
                    this.quoteAggregateResolverService.Object);

            // Act
            var fileAttachment = sut.GetFileAttachmentContent(quoteAggregate.TenantId, attachmentId);

            // Assert
            fileAttachment.HasValue.Should().BeTrue();
            fileAttachment.Value.FileContent.Should().BeEquivalentTo(contentInBytes);
            fileAttachment.Value.Name.Should().Be(filename);
        }

        [Fact]
        public void GetFileAttachmentContentForQuote_MissingFileAttachment_ShouldReturnNoValue()
        {
            // Arrange
            var attachmentId = Guid.NewGuid();
            var content = "Hello world!";
            var contentInBytes = System.Text.Encoding.ASCII.GetBytes(content);
            var base64 = Convert.ToBase64String(contentInBytes);
            var quote = QuoteAggregate.CreateNewBusinessQuote(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                DeploymentEnvironment.Development,
                QuoteExpirySettings.Default,
                default(Guid),
                this.clock.Object.Now(),
                Guid.NewGuid(),
                null,
                true);

            var quoteAggregate = quote.Aggregate;
            this.quoteAggregateResolverService
                .Setup(q => q.GetQuoteAggregateForQuote(quoteAggregate.TenantId, quote.Id))
                .Returns(quoteAggregate);

            this.fileAttachmentRepository
                .Setup(f => f.GetAttachmentContent(Guid.NewGuid(), attachmentId))
                .Returns(Maybe<IFileContentReadModel>.None);

            var sut =
                new ApplicationQuoteFileAttachmentService(
                    this.fileAttachmentRepository.Object,
                    this.quoteAggregateResolverService.Object);

            // Act
            var fileAttachment = sut.GetFileAttachmentContent(quoteAggregate.TenantId, attachmentId);

            // Assert
            fileAttachment.HasNoValue.Should().BeTrue();
        }
    }
}
