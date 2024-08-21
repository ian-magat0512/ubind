// <copyright file="SavedFileProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.FileHandling
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using MimeKit;
    using Moq;
    using NodaTime;
    using UBind.Application.FileHandling;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Events;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    /// <summary>
    /// Saved file provider unit tests.
    /// </summary>
    public class SavedFileProviderTests
    {
        private readonly Mock<IHttpContextPropertiesResolver> httpContextPropertiesResolver = new Mock<IHttpContextPropertiesResolver>();

        [Fact]
        public async Task SendExistingDocument_FromApplication_ShouldShowSameDocumentAsGenerated()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var quoteAggregate = QuoteFactory.CreateNewPolicy(tenantId);
            var quote = quoteAggregate.GetQuoteOrThrow(quoteAggregate.Policy.QuoteId.GetValueOrDefault());
            var policyEventAndIndex = this.GetPolicyEventAndSequenceNumber(quoteAggregate);
            var eventType = QuoteEventTypeMap.Map(policyEventAndIndex.Item1);
            var applicationEvent = new ApplicationEvent(
                Guid.NewGuid(),
                eventType.First(),
                quoteAggregate,
                quoteAggregate.Policy.QuoteId.GetValueOrDefault(),
                policyEventAndIndex.Item2,
                "1",
                quote.ProductReleaseId.Value);
            var fileContentRepository = new FakeFileContentRepository();
            var quoteAggregateRepository = new Mock<IQuoteAggregateRepository>();
            quoteAggregateRepository.Setup(r => r.GetById(quoteAggregate.TenantId, quoteAggregate.Id)).Returns(quoteAggregate);
            var documentName = "schedule.pdf";
            var contentType = ContentType.Parse("text/pdf");
            var attachmentContentStream = new MemoryStream(Encoding.UTF8.GetBytes("Hello World!"));
            var attachment = MimeEntity.Load(contentType, attachmentContentStream);
            attachment.ContentDisposition = new ContentDisposition(ContentDisposition.Attachment)
            {
                FileName = documentName,
            };
            var documentService = new ApplicationDocumentService(
                fileContentRepository,
                quoteAggregateRepository.Object,
                this.httpContextPropertiesResolver.Object,
                SystemClock.Instance,
                new Mock<ILogger<ApplicationDocumentService>>().Object);
            await documentService.AttachDocumentsAsync(applicationEvent, attachment);
            var outputName = "schedule.pdf";
            var sut = new SavedFileProvider("Policy", documentName, outputName, null, documentService);

            // Act
            var savedAttachment = await sut.Invoke(applicationEvent);

            // Assert
            Assert.Equal(outputName, savedAttachment.ContentDisposition.FileName);
            var attachmentStream = new MemoryStream();
            var savedAttachmentStream = new MemoryStream();
            attachmentContentStream.Position = 0;
            attachmentContentStream.CopyTo(attachmentStream);
            var part = (MimePart)attachment;
            part.Content.DecodeTo(savedAttachmentStream);

            savedAttachmentStream.Position = 0;
            var originalBytes = attachmentStream.GetBuffer();
            var savedBytes = savedAttachmentStream.GetBuffer();
            Assert.Equal(originalBytes.Length, savedBytes.Length);
            for (int index = 0; index < savedBytes.Length; ++index)
            {
                Assert.Equal(savedBytes[index], originalBytes[index]);
            }
        }

        private Tuple<IEvent<QuoteAggregate, Guid>, int> GetPolicyEventAndSequenceNumber(QuoteAggregate aggregate) =>
            aggregate.UnsavedEvents
                .Select((@event, index) => Tuple.Create(@event, index))
                .Where(pair => pair.Item1.GetType() == typeof(QuoteAggregate.PolicyIssuedEvent))
                .Single();

        private class FakeFileContentRepository : IFileContentRepository
        {
            private readonly Dictionary<Guid, FileContent> fileContents = new Dictionary<Guid, FileContent>();

            public FileContent GetFileContent(Guid tenantId, Guid quoteAttachmentOrFileContentId)
            {
                return this.GetFileContentById(quoteAttachmentOrFileContentId)
                    ?? this.GetFileContentByQuoteFileAttachmentId(tenantId, quoteAttachmentOrFileContentId);
            }

            public FileContent GetFileContentByHashCode(Guid tenantId, string hashCode)
            {
                return this.fileContents.First(f => f.Value.TenantId == tenantId && f.Value.HashCode == hashCode).Value;
            }

            public FileContent GetFileContentById(Guid id)
            {
                this.fileContents.TryGetValue(id, out FileContent fileContent);
                return fileContent;
            }

            public FileContent GetFileContentByQuoteFileAttachmentId(Guid tenantId, Guid attachmentId)
            {
                this.fileContents.TryGetValue(attachmentId, out FileContent fileContent);
                return fileContent;
            }

            public Guid? GetFileContentIdOrNullForHashCode(Guid tenantId, string hashCode)
            {
                return this.fileContents
                    .Where(f => f.Value.TenantId == tenantId && f.Value.HashCode == hashCode)
                    .Select(f => f.Value.Id)
                    .SingleOrDefault();
            }

            public bool HasFileContentWithHashCode(Guid tenantId, string hashCode)
            {
                return this.fileContents.Any(f => f.Value.TenantId == tenantId && f.Value.HashCode == hashCode);
            }

            public Guid Insert(FileContent fileContent)
            {
                bool found = false;
                var file = this.fileContents.FirstOrDefault(f => f.Value.HashCode == fileContent.HashCode && (found = true));
                if (!found)
                {
                    this.fileContents.Add(fileContent.Id, fileContent);
                }

                return found ? file.Value.Id : fileContent.Id;
            }

            public void PopulateFileContentsForQuoteFileAttachments()
            {
                // This is only used in the startup job, no code needed here.
            }

            public void ProcessBatch(int batch)
            {
                // This is only used in the startup job, no code needed here.
            }

            public void ProcessEventJsonBatch(int batch)
            {
                // This is only used in the startup job, no code needed here.
            }

            public void RemoveEventJsonFileContent()
            {
                // This is only used in the startup job, no code needed here.
            }

            public void RemoveEventJsonFileContentCleanup()
            {
                // This is only used in the startup job, no code needed here.
            }

            public void UpdateFileContentsAndHashCodes()
            {
                // This is only used in the startup job, no code needed here.
            }
        }
    }
}
