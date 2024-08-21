// <copyright file="RecreateReadModelsOfEventsMigration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Runtime;
    using System.Text;
    using System.Threading.Tasks;
    using Hangfire;
    using Hangfire.Server;
    using Microsoft.Extensions.Logging;
    using MoreLinq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Helpers;
    using UBind.Domain.Product;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.Migration;

    public class RecreateReadModelsOfEventsMigration : IRecreateReadModelsOfEventsMigration
    {
        private const int RecordsPerBatch = 2000;
        private const int RetryCount = 3;
        private readonly IProductRepository productRepository;
        private readonly IClock clock;
        private readonly IConnectionConfiguration connection;
        private readonly ITenantRepository tenantRepository;
        private readonly ICachingResolver cachingResolver;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IOrganisationAggregateRepository organisationAggregateRepository;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IUBindDbContext dbContext;
        private readonly ILogger<RecreateReadModelsOfEventsMigration> logger;
        private readonly IBackgroundJobClient jobClient;
        private readonly string connectionString;
        private readonly string tmpTableName = "AggregateMigrationTmp";

        /// <summary>
        /// For testing only.
        /// </summary>
        private List<Guid> testRecords = new List<Guid>
        {
        };

        public RecreateReadModelsOfEventsMigration(
            IUBindDbContext db,
            ILogger<RecreateReadModelsOfEventsMigration> logger,
            IBackgroundJobClient jobClient,
            IQuoteAggregateRepository quoteAggregateRepository,
            IUserAggregateRepository userAggregateRepository,
            IOrganisationAggregateRepository organisationAggregateRepository,
            IPersonAggregateRepository personAggregateRepository,
            ICachingResolver cachingResolver,
            ITenantRepository tenantRepository,
            IProductRepository productRepository,
            IClock clock,
            IConnectionConfiguration connection)
        {
            this.productRepository = productRepository;
            this.clock = clock;
            this.tenantRepository = tenantRepository;
            this.cachingResolver = cachingResolver;
            this.personAggregateRepository = personAggregateRepository;
            this.organisationAggregateRepository = organisationAggregateRepository;
            this.userAggregateRepository = userAggregateRepository;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.dbContext = db;
            this.logger = logger;
            this.jobClient = jobClient;
            this.connectionString = db.Database.Connection.ConnectionString;
            this.connection = connection;
        }

        public void Process()
        {
            using (var dbContext = new UBindDbContext(this.connection.UBind))
            {
                var whereClause = "where Sequence = 0 and TenantId = '00000000-0000-0000-0000-000000000000'";
                if (this.testRecords.Any())
                {
                    whereClause = $"where Sequence = 0 and AggregateId in ('{string.Join("','", this.testRecords)}')";
                }

                var createTempTableCommand = $"SELECT [AggregateId] as [AggregateId], [TenantId] AS [TenantId] INTO {this.tmpTableName} FROM EventRecordWithGuidIds " + whereClause;

                bool exists = dbContext.Database
                    .SqlQuery<int?>($"Select 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{this.tmpTableName}'")
                    .SingleOrDefault() != null;

                if (!exists)
                {
                    this.logger.LogInformation($"Populating {this.tmpTableName} list for batching purposes..");
                    dbContext.Database.ExecuteSqlCommand(createTempTableCommand, null, null, 43200, System.Data.CommandType.Text);
                }
            }

            this.jobClient.Enqueue(() => this.ProcessBatch(1, null));
        }

        [JobDisplayName("Process Batch: {0}")]
        public async Task ProcessBatch(
            int batchCount,
            PerformContext context)
        {
            var countQuery = $"SELECT COUNT(*) FROM {this.tmpTableName} WITH (NOLOCK)";
            var totalRecordsToProcess = this.dbContext.Database.SqlQuery<int>(countQuery).Single();
            this.logger.LogInformation($"Records to process: {totalRecordsToProcess}");

            int recordsSoFar = 0;
            var sqlBatchQuery = $"SELECT TOP({RecordsPerBatch}) AggregateId, TenantId FROM {this.tmpTableName} WITH (NOLOCK)";
            var currentBatch = this.dbContext.Database.SqlQuery<AggregateTenantPair>(sqlBatchQuery).ToList();
            if (currentBatch.Any())
            {
                foreach
                    (var batch in currentBatch)
                {
                    // add delay so that other db processing are not deadlocked.
                    await Task.Delay(100);
                    recordsSoFar++;

                    // only log every 25 records, temporarily set to true always for debugging.
                    if (recordsSoFar % 50 == 0)
                    {
                        this.logger.LogInformation($"Updating {recordsSoFar}/{totalRecordsToProcess} records of events for " + batch.AggregateId);
                    }

                    var aggregateQuery = $"SELECT TOP 1 AggregateId, EventJson, TicksSinceEpoch FROM EventRecordWithGuidIds WHERE AggregateId = '{batch.AggregateId}'";
                    AggregateItem aggregateItem = null;
                    void ExecuteSelectQuery()
                    {
                        try
                        {
                            aggregateItem = this.dbContext.Database.SqlQuery<AggregateItem>(aggregateQuery).FirstOrDefault();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex?.InnerException?.Message ?? ex.Message);
                        }
                    }

                    RetryPolicyHelper.Execute<Exception>(() => ExecuteSelectQuery(), RetryCount, 50, 100);
                    var aggregateType = this.GetAggregateTypeOfEventJson(aggregateItem.EventJson);
                    var creationDate = aggregateItem.CreationDate.InUtc().ToString();

                    EventRecordWithGuidId evenRecord =
                        new EventRecordWithGuidId(batch.TenantId, batch.AggregateId, 0, aggregateItem.EventJson, aggregateType, aggregateItem.CreationDate);
                    int eventsCount = 0;
                    var command = new StringBuilder();
                    var deleteAggregateRecords = false;
                    try
                    {
                        switch (aggregateType)
                        {
                            case AggregateType.User:
                                var user = this.dbContext.Users.FirstOrDefault(x => x.Id == batch.AggregateId);

                                if (user == null)
                                {
                                    this.logger.LogInformation($"Aggregate 'User' - {batch.AggregateId}");
                                    var @event = evenRecord.GetEvent<IEvent<UserAggregate, Guid>, UserAggregate>();
                                    var id = await this.GetTenantId(@event, aggregateItem.EventJson);
                                    var tenantId = @event.TenantId = batch.TenantId = id.Value;
                                    await this.userAggregateRepository.ReplayAllEventsByAggregateId(@event.TenantId, batch.AggregateId, overrideTenantId: tenantId);
                                }

                                break;
                            case AggregateType.Person:
                                var person = this.dbContext.PersonReadModels.FirstOrDefault(x => x.Id == batch.AggregateId);

                                if (person == null)
                                {
                                    this.logger.LogInformation($"Aggregate 'Person' - {batch.AggregateId}");
                                    var @event = evenRecord.GetEvent<IEvent<PersonAggregate, Guid>, PersonAggregate>();
                                    var id = await this.GetTenantId(@event, aggregateItem.EventJson);
                                    var tenantId = @event.TenantId = batch.TenantId = id.Value;
                                    await this.personAggregateRepository.ReplayAllEventsByAggregateId(@event.TenantId, batch.AggregateId, overrideTenantId: tenantId);
                                }

                                break;
                            case AggregateType.Quote:
                                var quoteAggregate = this.quoteAggregateRepository.GetById(batch.TenantId, batch.AggregateId);
                                var quoteFromAggregate = quoteAggregate.GetLatestQuote();
                                var quoteId = quoteFromAggregate?.Id;
                                var quote = quoteId == null ? null : this.dbContext.QuoteReadModels.FirstOrDefault(x => x.Id == quoteId);

                                if (((quoteAggregate.Environment == DeploymentEnvironment.Development
                                    || quoteAggregate.Environment == DeploymentEnvironment.Staging)
                                    && quote == null)
                                ||
                                    (quoteAggregate.Environment == DeploymentEnvironment.Production
                                    && !quoteAggregate.GetQuotes().Any(x => x.QuoteStatus.ToLower() != "nascent")
                                    && quoteAggregate.Policy == null))
                                {
                                    // delete aggregate completely.
                                    command.AppendLine($"DELETE EventRecordWithGuidIds WHERE AggregateId = '{batch.AggregateId}';");
                                    deleteAggregateRecords = true;
                                    break;
                                }

                                if (quoteAggregate.Environment == DeploymentEnvironment.Production
                                    && quoteFromAggregate?.QuoteStatus?.ToLower() == "nascent"
                                    && quoteAggregate.Policy == null)
                                {
                                    break;
                                }

                                if (quote == null)
                                {
                                    this.logger.LogInformation($"Creating ReadModel for Quote Aggregate with Id = " + batch.AggregateId);
                                    var @event = evenRecord.GetEvent<IEvent<QuoteAggregate, Guid>, QuoteAggregate>();
                                    Guid? tenantId = await this.GetTenantId(@event, aggregateItem.EventJson);
                                    @event.TenantId = batch.TenantId = tenantId.Value;
                                    await this.quoteAggregateRepository.ReplayAllEventsByAggregateId(@event.TenantId, batch.AggregateId, overrideTenantId: tenantId);
                                }

                                break;
                            case AggregateType.Organisation:
                                var organisation = this.dbContext.OrganisationReadModel.FirstOrDefault(x => x.Id == batch.AggregateId);

                                if (organisation == null)
                                {
                                    this.logger.LogInformation($"Aggregate 'Organisation' - {batch.AggregateId}");
                                    var @event = evenRecord.GetEvent<IEvent<Organisation, Guid>, Organisation>();
                                    Guid? tenantId = await this.GetTenantId(@event, aggregateItem.EventJson);
                                    @event.TenantId = batch.TenantId = tenantId.Value;
                                    await this.organisationAggregateRepository.ReplayAllEventsByAggregateId(@event.TenantId, batch.AggregateId, overrideTenantId: tenantId);
                                }

                                break;

                            default:
                                this.logger.LogInformation($"AGGREGATE UNIDENTIFIED/UNPROCESSED - {aggregateType} {batch.AggregateId}");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogInformation($"Error - AggregateId:{batch.AggregateId} ERROR caused by {ex.Message} - {ex?.InnerException?.Message} EventsCount:{eventsCount}");
                    }

                    if (batch.TenantId != Guid.Empty
                        && !deleteAggregateRecords)
                    {
                        command.AppendLine($"UPDATE EventRecordWithGuidIds SET TenantId = '{batch.TenantId}' WHERE AggregateId = '{batch.AggregateId}';");
                    }

                    command.AppendLine($"DELETE FROM {this.tmpTableName} WHERE " + $"AggregateId = '{batch.AggregateId}';");
                    void ExecuteUpdate()
                    {
                        if (command.Length > 0)
                        {
                            try
                            {
                                this.dbContext.Database.ExecuteSqlCommand(command.ToString());
                                this.dbContext.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                this.logger.LogInformation($"Error - AggregateId:{batch.AggregateId} ERROR caused by {ex.Message} - {ex?.InnerException?.Message}");
                            }
                        }
                    }

                    RetryPolicyHelper.Execute<Exception>(() => ExecuteUpdate(), RetryCount, 50, 75);
                }
            }

            if (totalRecordsToProcess - currentBatch.Count > 0)
            {
                // every 5 batches we shrink.
                if (batchCount % 5 == 0)
                {
                    this.ShrinkLogs();
                    this.DeallocateMemory();
                }

                batchCount++;
                this.jobClient.ContinueJobWith<RecreateReadModelsOfEventsMigration>(
                    context.BackgroundJob.Id,
                    j => j.ProcessBatch(batchCount, null));
            }
            else
            {
                this.ShrinkLogs();
                this.DeallocateMemory();
                this.logger.LogInformation($"{recordsSoFar}/{totalRecordsToProcess} Finished... Cleaning up.");

                // CLEANUP.
                var dropTempCommand = $"DROP TABLE {this.tmpTableName}";
                this.dbContext.Database.ExecuteSqlCommand(dropTempCommand);
            }
        }

        private void ShrinkLogs()
        {
            using (var dbContext = new UBindDbContext(this.connection.UBind))
            {
                this.logger.LogInformation("Shrinking DB log...");
                dbContext.Database.ExecuteSqlCommand(
                    TransactionalBehavior.DoNotEnsureTransaction, "DBCC SHRINKFILE (2, TRUNCATEONLY)");
            }
        }

        private void DeallocateMemory()
        {
            var memBefore = this.BytesToGb(GC.GetTotalMemory(false));
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Default, true, true);
            GC.WaitForPendingFinalizers();
            var memAfter = this.BytesToGb(GC.GetTotalMemory(true));

            this.logger.LogInformation(
                $"GC starts with {memBefore:N} GB, ends with {memAfter:N} GB");
        }

        private AggregateType GetAggregateTypeOfEventJson(string eventJson)
        {
            eventJson = eventJson.ToLower();

            if (eventJson.Contains("user.useraggregate"))
            {
                return AggregateType.User;
            }

            if (eventJson.Contains("quote.quoteaggregate"))
            {
                return AggregateType.Quote;
            }

            if (eventJson.Contains("person.personaggregate"))
            {
                return AggregateType.Person;
            }

            if (eventJson.Contains("customer.customeraggregate"))
            {
                return AggregateType.Customer;
            }

            if (eventJson.Contains("claim.claimaggregate"))
            {
                return AggregateType.Claim;
            }

            if (eventJson.Contains("organisation.organisation"))
            {
                return AggregateType.Organisation;
            }

            if (eventJson.Contains("report.reportaggregate"))
            {
                return AggregateType.Report;
            }

            if (eventJson.Contains("additionalpropertyvalue.textadditionalpropertyvalue"))
            {
                return AggregateType.TextAdditionalPropertyValue;
            }

            if (eventJson.Contains("additionalpropertydefinition.additionalpropertydefinition"))
            {
                return AggregateType.AdditionalPropertyDefinition;
            }

            if (eventJson.Contains("accounting.paymentaggregate"))
            {
                return AggregateType.FinancialTransaction;
            }

            if (eventJson.Contains("accounting.refundaggregate"))
            {
                return AggregateType.FinancialTransaction;
            }

            if (eventJson.Contains("accounting.invoiceaggregate"))
            {
                return AggregateType.FinancialTransaction;
            }

            if (eventJson.Contains("accounting.financialtransactionaggregate"))
            {
                return AggregateType.FinancialTransaction;
            }

            return default;
        }

        private async Task<Guid?> GetTenantId<TAggregateRootEntity>(IEvent<TAggregateRootEntity, Guid> @event, string eventJson)
            where TAggregateRootEntity : IAggregateRootEntity<TAggregateRootEntity, Guid>
        {
            Guid? tenantId = null;

            if (@event.TenantId != Guid.Empty)
            {
                tenantId = @event.TenantId;
            }
            else
            {
                var eventObj = JsonConvert.DeserializeObject<dynamic>(eventJson);

                string tenantStringId =
                    eventObj.TenantStringId != null ?
                    eventObj.TenantStringId
                    : eventObj.Person != null ?
                        eventObj.Person.TenantString?.ToString()
                        : eventObj.PersonData != null ?
                            eventObj.PersonData.TenantString?.ToString()
                            : null;
                string tenantId1 =
                    eventObj.TenantNewId != null ?
                    eventObj.TenantNewId
                    : eventObj.Person != null ?
                        eventObj.Person.TenantNewId?.ToString()
                        : eventObj.PersonData != null ?
                            eventObj.PersonData.TenantNewId?.ToString()
                            : null;
                string tenantId2 =
                    eventObj.TenantId != null ?
                    eventObj.TenantId
                    : eventObj.Person != null ?
                        eventObj.Person.TenantId?.ToString()
                        : eventObj.PersonData != null ?
                            eventObj.PersonData.TenantId?.ToString()
                            : null;

                if (!string.IsNullOrEmpty(tenantStringId) && tenantId == null)
                {
                    var guidOrAlias = new GuidOrAlias(tenantStringId);
                    if (guidOrAlias.Guid == null)
                    {
                        tenantId = this.tenantRepository.GetTenantByStringId(tenantStringId)?.Id;
                    }
                    else
                    {
                        var tenant = await this.cachingResolver.GetTenantOrNull(new GuidOrAlias(tenantStringId));
                        tenantId = tenant?.Id;
                    }
                }

                if (!string.IsNullOrEmpty(tenantId1) && tenantId == null)
                {
                    var guidOrAlias = new GuidOrAlias(tenantId1);
                    if (guidOrAlias.Guid == null)
                    {
                        tenantId = this.tenantRepository.GetTenantByStringId(tenantId1)?.Id;
                    }
                    else
                    {
                        var tenant = await this.cachingResolver.GetTenantOrNull(new GuidOrAlias(tenantId1));
                        tenantId = tenant?.Id;
                    }
                }

                if (!string.IsNullOrEmpty(tenantId2) && tenantId == null)
                {
                    var guidOrAlias = new GuidOrAlias(tenantId2);
                    if (guidOrAlias.Guid == null)
                    {
                        tenantId = this.tenantRepository.GetTenantByStringId(tenantId2)?.Id;
                    }
                    else
                    {
                        var tenant = await this.cachingResolver.GetTenantOrNull(new GuidOrAlias(tenantId2));
                        tenantId = tenant?.Id;
                    }
                }
            }

            return tenantId;
        }

        private async Task<Guid> GetProductId(Guid tenantId, string eventJson)
        {
            Guid? productId = null;
            var eventObj = JsonConvert.DeserializeObject<dynamic>(eventJson);

            string productStringId =
                eventObj.ProductStringId != null ?
                eventObj.ProductStringId
                : eventObj.Person != null ?
                    eventObj.Person.ProductStringId?.ToString()
                    : eventObj.PersonData != null ?
                        eventObj.PersonData.ProductStringId?.ToString()
                        : null;
            string productId1 =
                eventObj.ProductNewId != null ?
                eventObj.ProductNewId
                : eventObj.Person != null ?
                    eventObj.Person.ProductNewId?.ToString()
                    : eventObj.PersonData != null ?
                        eventObj.PersonData.ProductNewId?.ToString()
                        : null;
            string productId2 =
                eventObj.ProductId != null ?
                eventObj.ProductId
                : eventObj.Person != null ?
                    eventObj.Person.ProductId?.ToString()
                    : eventObj.PersonData != null ?
                        eventObj.PersonData.ProductId?.ToString()
                        : null;

            if (!string.IsNullOrEmpty(productStringId) && productId == null)
            {
                var guidOrAlias = new GuidOrAlias(productStringId);
                if (guidOrAlias.Guid == null)
                {
                    productId = this.productRepository.GetProductByStringId(tenantId, productStringId)?.Id;
                }
                else
                {
                    var product = await this.cachingResolver.GetProductOrNull(tenantId, new GuidOrAlias(productStringId));
                    productId = product?.Id;
                }
            }

            if (!string.IsNullOrEmpty(productId1) && productId == null)
            {
                var guidOrAlias = new GuidOrAlias(productId1);
                if (guidOrAlias.Guid == null)
                {
                    productId = this.productRepository.GetProductByStringId(tenantId, productId1)?.Id;
                }
                else
                {
                    var product = await this.cachingResolver.GetProductOrNull(tenantId, new GuidOrAlias(productId1));
                    productId = product?.Id;
                }
            }

            if (!string.IsNullOrEmpty(productId2) && productId == null)
            {
                var guidOrAlias = new GuidOrAlias(productId2);
                if (guidOrAlias.Guid == null)
                {
                    productId = this.productRepository.GetProductByStringId(tenantId, productId2)?.Id;
                }
                else
                {
                    var product = await this.cachingResolver.GetProductOrNull(tenantId, new GuidOrAlias(productId2));
                    productId = product?.Id;
                }
            }

            return productId.Value;
        }

        private double BytesToGb(long bytes)
        {
            return bytes * 1E-9;
        }

        private class AggregateTenantPair
        {
            public Guid AggregateId { get; set; }

            public Guid TenantId { get; set; }
        }

        private class DistinctAggregate
        {
            public Guid AggregateId { get; set; }
        }

        private class AggregateItem
        {
            public Guid AggregateId { get; set; }

            public string EventJson { get; set; }

            public long TicksSinceEpoch { get; set; }

            /// <summary>
            /// Gets the record creation timestamp.
            /// </summary>
            public Instant CreationDate => Instant.FromUnixTimeTicks(this.TicksSinceEpoch);
        }
    }
}
