// <copyright file="ApplicationDocumentServiceIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.IntegrationTests.Application.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using MimeKit;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Extensions;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class ApplicationDocumentServiceIntegrationTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();
        private IClock clock = SystemClock.Instance;

        // The transaction in DatabaseIntegrationTests is used to rollback change from each database integration test before executing the next one. However, it will prevent database changes from being readable in other concurrent threads, and so will test like this onen fail. It can be temporarily turned off for manually running tests like this one.
        [Fact(Skip = "This test needs to be run outside the transaction setup in DatabaseIntegrationTests in order to work.")]
        public async Task AttachDocuments_PersistsDocumentContent_EvenWhenSavingAggregateRequiresRetry()
        {
            // Arrange
            Guid aggregateId;
            Guid quoteId;
            Guid tenantId = Guid.NewGuid();
            QuoteAggregate quoteAggregate;
            using (var stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var quote = QuoteFactory.CreateNewBusinessQuote();
                quoteId = quote.Id;
                aggregateId = quote.Aggregate.Id;
                quoteAggregate = quote.Aggregate;
                await stack.QuoteAggregateRepository.Save(quote.Aggregate);
            }

            // Reload quote.
            var stack2 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var reloadedAggregate = stack2.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, aggregateId);
            var quoteReload = reloadedAggregate.GetQuoteOrThrow(quoteId);
            var applicationEvent = new ApplicationEvent(
                Guid.NewGuid(),
                ApplicationEventType.QuoteCreated,
                reloadedAggregate,
                quoteReload.Id,
                0,
                "1",
                quoteReload.ProductReleaseId.Value);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes("Test file content"));
            var doc = MimeEntity.Load(stream);

            // Make the first three attempts to persist docs and aggregate fail due to concurrency exception.
            var numberOfFails = 3;
            void MakeConcurrentUpdate()
            {
                if (numberOfFails > 0)
                {
                    var thread = new Thread(async () =>
                    {
                        var stack3 = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
                        var parallelQuote = stack3.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, aggregateId);
                        var quote = parallelQuote.GetQuoteOrThrow(quoteId);
                        quote.UpdateFormData(new Domain.Aggregates.Quote.FormData(FormDataJsonFactory.Sample), this.performingUserId, this.clock.Now());
                        await stack3.QuoteAggregateRepository.Save(parallelQuote);
                    });
                    thread.Start();
                    thread.Join();
                }

                --numberOfFails;
            }

            stack2.DbContext.BeforeSaveChanges += dbContext => MakeConcurrentUpdate();

            // Act
            await stack2.ApplicationDocumentService.AttachDocumentsAsync(applicationEvent, doc);

            // Assert
            using (var stack4 = new ApplicationStack(DatabaseFixture.TestConnectionStringName))
            {
                var persistedAggregate = stack4.QuoteAggregateRepository.GetById(quoteAggregate.TenantId, aggregateId);
                var quotePersisted = persistedAggregate.GetQuoteOrThrow(quoteReload.Id);
                Assert.Single(quotePersisted.Documents); // "Quote should only have a single documents persisted.");
                var attachedDocument = quotePersisted.Documents.FirstOrDefault();
                var filecontent = stack4.FileContentRepository.GetFileContentById(attachedDocument.FileContentId);
                filecontent.Should().NotBeNull("File content should be persisted.");
                Assert.Single(stack4.DbContext.FileContents); // "Only a single copy of the file content should be persisted.");
            }
        }
    }
}
