// <copyright file="ApplicationDocumentServiceTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MimeKit;
using Moq;
using NodaTime;
using UBind.Application.Services;
using UBind.Domain;
using UBind.Domain.Aggregates;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Events;
using UBind.Domain.Repositories;
using UBind.Domain.Tests.Fakes;
using Xunit;

public class ApplicationDocumentServiceTest
{
    private readonly Mock<IFileContentRepository> fileContentRepository;
    private readonly Mock<IQuoteAggregateRepository> quoteAggregateRepository;
    private readonly Mock<ILogger<ApplicationDocumentService>> logger;
    private readonly Mock<IHttpContextPropertiesResolver> httpContextPropertiesResolver;
    private readonly IClock clock;
    private readonly ApplicationDocumentService documentService;

    public ApplicationDocumentServiceTest()
    {
        this.clock = SystemClock.Instance;
        this.fileContentRepository = new Mock<IFileContentRepository>();
        this.quoteAggregateRepository = new Mock<IQuoteAggregateRepository>();
        this.httpContextPropertiesResolver = new Mock<IHttpContextPropertiesResolver>();
        this.logger = new Mock<ILogger<ApplicationDocumentService>>();
        this.documentService = new ApplicationDocumentService(
            this.fileContentRepository.Object,
            this.quoteAggregateRepository.Object,
            this.httpContextPropertiesResolver.Object,
            this.clock,
            this.logger.Object);
    }

    [Fact]
    public async Task AttachDocumentsAsync_ShouldSucceed_WhenSavingNewDocument()
    {
        // Arrange
        var (quoteAggregate, applicationEvent) = this.PrepareTestData();
        ContentType contentType = ContentType.Parse("text/plain");
        var document = MimeEntity.Load(contentType, new MemoryStream(new byte[123]));
        document.ContentDisposition = new ContentDisposition(ContentDisposition.Attachment);
        document.ContentDisposition.FileName = "test doc";

        // Act
        await this.documentService.AttachDocumentsAsync(applicationEvent, document);

        // Assert
        var testDoc = quoteAggregate.GetPolicyDocument("test doc");
        testDoc.Should().NotBeNull();
        testDoc.SizeInBytes.Should().Be(((MimePart)document).Content.Stream.Length);
        testDoc.Type.Should().Be(document.ContentType.MimeType);
    }

    [Fact]
    public async Task AttachDocumentsAsync_ShouldSucceed_WhenExistingDocumentDetectedWithSameNameAndType()
    {
        // Arrange
        var (quoteAggregate, applicationEvent) = this.PrepareTestData();
        ContentType contentType = ContentType.Parse("text/plain");
        var document = MimeEntity.Load(
            contentType, new MemoryStream(new byte[123]));
        document.ContentDisposition = new ContentDisposition(ContentDisposition.Attachment);
        document.ContentDisposition.FileName = "test doc";

        var sameDocument = MimeEntity.Load(
            contentType, new MemoryStream(new byte[1234]));
        sameDocument.ContentDisposition = new ContentDisposition(ContentDisposition.Attachment);
        sameDocument.ContentDisposition.FileName = "test doc";

        // Act
        await this.documentService.AttachDocumentsAsync(applicationEvent, document);
        await this.documentService.AttachDocumentsAsync(applicationEvent, sameDocument);

        // Assert
        var testDoc = quoteAggregate.GetPolicyDocument("test doc");
        testDoc.Should().NotBeNull();
        testDoc.SizeInBytes.Should().Be(((MimePart)sameDocument).Content.Stream.Length);
        testDoc.Type.Should().Be(sameDocument.ContentType.MimeType);
    }

    [Fact]
    public async Task AttachDocumentsAsync_ShouldSucceed_WhenDocumentOfSameNameButDifferentTypeExists()
    {
        // Arrange
        var (quoteAggregate, applicationEvent) = this.PrepareTestData();
        ContentType contentType = ContentType.Parse("text/plain");
        var document = MimeEntity.Load(contentType, new MemoryStream(new byte[123]));
        document.ContentDisposition = new ContentDisposition(ContentDisposition.Attachment);
        document.ContentDisposition.FileName = "test doc";

        // Act
        await this.documentService.AttachDocumentsAsync(applicationEvent, document);

        // Assert
        var testDoc = quoteAggregate.GetPolicyDocument("test doc");
        testDoc.Should().NotBeNull();
        testDoc.SizeInBytes.Should().Be(((MimePart)document).Content.Stream.Length);
        testDoc.Type.Should().Be(document.ContentType.MimeType);
    }

    [Fact]
    public async Task AttachDocumentsAsync_ShouldNotThrowException_WhenRetrying()
    {
        // Arrange
        var (quoteAggregate, applicationEvent) = this.PrepareTestData();
        ContentType contentType = ContentType.Parse("text/plain");
        var document = MimeEntity.Load(contentType, new MemoryStream(Encoding.UTF8.GetBytes("Leon")));
        document.ContentDisposition = new ContentDisposition(ContentDisposition.Attachment)
        {
            FileName = "test doc",
        };

        // Act
        // Simulate retrying attaching document by attaching the same document twice
        await this.documentService.AttachDocumentsAsync(applicationEvent, document);
        await this.documentService.AttachDocumentsAsync(applicationEvent, document);

        // Assert
        var testDoc = quoteAggregate.GetPolicyDocument("test doc");
        testDoc.Should().NotBeNull();
        testDoc.SizeInBytes.Should().Be(((MimePart)document).Content.Stream.Length);
        testDoc.Type.Should().Be(document.ContentType.MimeType);
    }

    private (QuoteAggregate, ApplicationEvent) PrepareTestData()
    {
        QuoteAggregate quoteAggregate = QuoteFactory.CreateNewPolicy();
        var quote = quoteAggregate.GetQuoteOrThrow(quoteAggregate.Policy.QuoteId.GetValueOrDefault());
        var policyEventAndIndex = this.GetPolicyEventAndSequenceNumber(quoteAggregate);
        List<ApplicationEventType> eventType = QuoteEventTypeMap.Map(policyEventAndIndex.Item1);
        var applicationEvent = new ApplicationEvent(
            Guid.NewGuid(),
            eventType.First(),
            quoteAggregate,
            quoteAggregate.Policy.QuoteId.GetValueOrDefault(),
            policyEventAndIndex.Item2,
            "1",
            quote.ProductReleaseId.Value);
        this.quoteAggregateRepository
            .Setup(r => r.GetById(quoteAggregate.TenantId, quoteAggregate.Id))
            .Returns(quoteAggregate);
        return (quoteAggregate, applicationEvent);
    }

    private Tuple<IEvent<QuoteAggregate, Guid>, int> GetPolicyEventAndSequenceNumber(QuoteAggregate aggregate) =>
        aggregate.UnsavedEvents
            .Select((@event, index) => Tuple.Create(@event, index))
            .Where(pair => pair.Item1.GetType() == typeof(QuoteAggregate.PolicyIssuedEvent))
            .Single();
}
