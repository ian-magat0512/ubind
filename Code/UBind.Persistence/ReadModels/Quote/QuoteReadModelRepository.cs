// <copyright file="QuoteReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Quote
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Transactions;
    using Dapper;
    using LinqKit;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Search;
    using UBind.Domain.ValueTypes;
    using UBind.Persistence;
    using UBind.Persistence.Extensions;
    using UBind.Persistence.ReadModels;
    using UBind.Persistence.Search;

    /// <inheritdoc />
    public class QuoteReadModelRepository : IQuoteReadModelRepository
    {
        private readonly IUBindDbContext dbContext;
        private readonly IClock clock;
        private readonly IConnectionConfiguration connection;
        private long currentTicksSinceEpoch;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteReadModelRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="connection">The conn strings.</param>
        /// <param name="clock">A clock.</param>
        public QuoteReadModelRepository(
            IUBindDbContext dbContext,
            IConnectionConfiguration connection,
            IClock clock)
        {
            this.dbContext = dbContext;
            this.connection = connection;
            this.clock = clock;
            this.currentTicksSinceEpoch = this.clock.Now().ToUnixTimeTicks();
        }

        /// <summary>
        /// Gets an expression for instantiating report summaries from QuoteReadModels for use in EF projections.
        /// </summary>
        private Expression<Func<QuoteProductOrganisationReadModel, IQuoteReportItem>> ReportItemSelector =>
            q => new QuoteReportItem
            {
                PolicyId = q.Quote.PolicyId,
                AggregateId = q.Quote.AggregateId,
                QuoteId = q.Quote.Id,
                TenantId = q.Quote.TenantId,
                CreatedTicksSinceEpoch = q.Quote.CreatedTicksSinceEpoch,
                LastModifiedTicksSinceEpoch = q.Quote.LastModifiedTicksSinceEpoch,
                ProductId = q.Quote.ProductId,
                OwnerUserId = q.Quote.OwnerUserId,
                CustomerId = q.Quote.CustomerId,
                CustomerFullName = q.Quote.CustomerFullName,
                CustomerPreferredName = q.Quote.CustomerPreferredName,
                CustomerEmail = q.Quote.CustomerEmail,
                CustomerAlternativeEmail = q.Quote.CustomerAlternativeEmail,
                CustomerMobilePhone = q.Quote.CustomerMobilePhone,
                CustomerHomePhone = q.Quote.CustomerHomePhone,
                CustomerWorkPhone = q.Quote.CustomerWorkPhone,
                QuoteNumber = q.Quote.QuoteNumber,
                IsSubmitted = q.Quote.IsSubmitted,
                IsTestData = q.Quote.IsTestData,
                SerializedLatestCalculationResult = q.Quote.SerializedLatestCalculationResult,
                QuoteType = q.Quote.Type,
                QuoteState = q.Quote.QuoteState,
                ProductAlias = q.Product.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Alias,
                ProductName = q.Product.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Name,
                PolicyNumber = q.Quote.PolicyNumber,
                InvoiceNumber = q.Quote.InvoiceNumber,
                InvoiceTicksSinceEpoch = q.Quote.InvoiceTicksSinceEpoch,
                SumbmissionTicksSinceEpoch = q.Quote.SubmissionTicksSinceEpoch,
                IsInvoiced = q.Quote.IsInvoiced,
                IsPaidFor = q.Quote.IsPaidFor,
                LatestFormData = q.Quote.LatestFormData,
                CreditNoteNumber = q.Quote.CreditNoteNumber,
                Environment = q.Quote.Environment,
                PaymentGateway = q.Quote.PaymentGateway,
                PaymentResponseJson = q.Quote.PaymentResponseJson,
                OrganisationName = q.Organisation.Name,
                OrganisationAlias = q.Organisation.Alias,
                AgentName = q.Quote.OwnerFullName,
            };

        /// <inheritdoc/>
        public bool HasQuotesForCustomer(QuoteReadModelFilters filters, IEnumerable<Guid> excludedQuoteIds)
        {
            return this.dbContext.QuoteReadModels.Any(q => q.TenantId == filters.TenantId
                && (filters.OrganisationIds.Any() ? filters.OrganisationIds.Contains(q.OrganisationId) : true)
                && q.Environment == filters.Environment
                && q.CustomerId == filters.CustomerId
                && !excludedQuoteIds.Contains(q.Id));
        }

        /// <inheritdoc/>
        public IEnumerable<IQuoteReadModelSummary> ListQuotes(Guid tenantId, QuoteReadModelFilters filters)
        {
            var query = this.QueryQuoteReadModels(tenantId, filters);
            return this.ProjectSummary(query, filters);
        }

        /// <inheritdoc/>
        public IEnumerable<Guid> ListQuoteIdsFromPolicy(Guid tenantId, Guid policyId, DeploymentEnvironment environment)
        {
            var query = this.dbContext.QuoteReadModels
                .Where(q => q.TenantId == tenantId)
                .Where(q => q.PolicyId == policyId)
                .Where(q => q.Environment == environment)
                .Select(q => q.Id);
            return query.ToList();
        }

        /// <inheritdoc/>
        public IEnumerable<Guid> ListQuoteIdsFromCustomer(Guid tenantId, Guid customerId, DeploymentEnvironment environment)
        {
            var query = this.dbContext.QuoteReadModels.Where(q =>
                q.TenantId == tenantId
                && q.CustomerId == customerId
                && q.Environment == environment).Select(q => q.Id);
            return query.ToList();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<QuoteDashboardSummaryModel>> ListQuotesForPeriodicSummary(Guid tenantId, QuoteReadModelFilters filters, CancellationToken cancellationToken)
        {
            filters.TenantId = tenantId;
            return await this.QueryQuotesForPeriodicSummary(filters, cancellationToken);
        }

        /// <inheritdoc/>
        public int CountQuotes(Guid tenantId, QuoteReadModelFilters filters)
        {
            var query = this.QueryQuoteReadModels(tenantId, filters);
            return query.Count();
        }

        /// <inheritdoc/>
        public int GetQuoteCountBetweenDates(
            Guid tenantId,
            DeploymentEnvironment environment,
            Instant fromDateTime,
            Instant toDateTime)
        {
            var fromDateTimeTicksSinceEpoch = fromDateTime.ToUnixTimeTicks();
            var toDateTimeTicksSinceEpoch = toDateTime.ToUnixTimeTicks();

            var query = this.dbContext.QuoteReadModels.Where(
                q => q.TenantId == tenantId
                && q.Environment == environment
                && q.QuoteState != StandardQuoteStates.Nascent
                && q.QuoteNumber != null
                && q.LastModifiedTicksSinceEpoch >= fromDateTimeTicksSinceEpoch
                && q.LastModifiedTicksSinceEpoch <= toDateTimeTicksSinceEpoch);

            return query.Count();
        }

        /// <inheritdoc/>
        public IEnumerable<IQuoteSearchIndexWriteModel> GetQuotesForSearchIndexCreation(
            Guid tenantId,
            DeploymentEnvironment environment,
            EntityListFilters filters,
            long? searchIndexLastUpdatedTicksSinceEpoch = null)
        {
            try
            {
                long lastUpdatedTicksSinceEpoch = 0;
                if (searchIndexLastUpdatedTicksSinceEpoch.HasValue)
                {
                    lastUpdatedTicksSinceEpoch = searchIndexLastUpdatedTicksSinceEpoch.Value;
                }

                var parameters = new DynamicParameters();
                parameters.Add("@LastModifiedTicksSinceEpoch", lastUpdatedTicksSinceEpoch);
                parameters.Add("@TenantId", tenantId);
                parameters.Add("@Environment", environment);
                parameters.Add("@Offset", filters.PageSize * (filters.Page - 1));
                parameters.Add("@Next", filters.PageSize);

                // set the formdata to null because we are not using it.
                string sql = @"
                    SELECT
                        q.id,
                        q.OrganisationId,
                        q.LastModifiedTicksSinceEpoch,
                        q.LastModifiedByUserTicksSinceEpoch,
                        q.CreatedTicksSinceEpoch,
                        q.[Type] [QuoteType],
                        q.LatestFormData [FormDataJson], 
                        q.QuoteNumber, 
                        q.QuoteState,
                        q.policyId,
                        q.ExpiryTicksSinceEpoch,
                        q.OwnerUserId,
                        q.OwnerPersonId,
                        q.OwnerFullName,
                        q.CustomerId, 
                        q.CustomerFullName, 
                        q.QuoteTitle,  
                        q.CustomerAlternativeEmail,
                        q.CustomerHomePhone,
                        q.CustomerMobilePhone,
                        q.CustomerWorkPhone,
                        q.CustomerPreferredName,
                        q.ProductId,
                        q.IsDiscarded,
                        q.IsTestData,
                        q.ProductId,
                        q.PolicyNumber
                    FROM Quotes q
                    WHERE
                            q.LastModifiedTicksSinceEpoch > @LastModifiedTicksSinceEpoch
                        AND q.TenantId = @TenantId
                        AND q.Environment = @Environment
                        AND q.QuoteState <> 'Nascent'
                        AND q.QuoteNumber IS NOT NULL
                    ORDER BY q.LastModifiedTicksSinceEpoch
                    OFFSET @Offset ROWS
                    FETCH NEXT @Next ROWS ONLY";

                using (var connection = new SqlConnection(this.connection.UBind))
                {
                    List<QuoteSearchIndexWriteModel> writeModels = new List<QuoteSearchIndexWriteModel>();

                    void QueryWriteModel()
                    {
                        writeModels = connection.Query<QuoteSearchIndexWriteModel>(sql, parameters, null, true, 180, System.Data.CommandType.Text).ToList();
                    }

                    RetryPolicyHelper.Execute<Exception>(() => QueryWriteModel(), maxJitter: 2000);

                    return writeModels;
                }
            }
            catch (Exception ex)
            {
                string innerExceptionMessage = string.Empty;

                if (ex.InnerException != null)
                {
                    innerExceptionMessage = $"{ex.InnerException.Message}.{ex.InnerException.StackTrace}";
                }

                throw new LuceneIndexingException($"Error in getting the Quotes from Database. {innerExceptionMessage} Tenant : {tenantId}, Environment : {environment}, LastUpdatedTickSinceEpoch: {searchIndexLastUpdatedTicksSinceEpoch}, Details: {ex.Message}.", ex);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<IQuoteReadModelSummary> GetQuotesByTenantAndProduct(
            Guid tenantId, Guid productId, DeploymentEnvironment environment, EntityListFilters filters)
        {
            var query = this.JoinQuoteProductOrganisation(this.dbContext.QuoteReadModels);
            return this.ProjectSummary(query).Where(q => q.TenantId == tenantId && q.ProductId == productId);
        }

        /// <inheritdoc/>
        public NewQuoteReadModel GetById(Guid tenantId, Guid? quoteId)
        {
            var quote = this.dbContext.QuoteReadModels.FirstOrDefault(rm => rm.TenantId == tenantId && rm.Id == quoteId);
            return quote;
        }

        public Guid? GetProductReleaseId(Guid tenantId, Guid quoteId)
        {
            return this.dbContext.QuoteReadModels
                .Where(q => q.TenantId == tenantId && q.Id == quoteId)
                .Select(q => q.ProductReleaseId)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets summary details of a given quote (for permission checks).
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the quote belongs to.</param>
        /// <param name="quoteId">The ID of the quote to fetch.</param>
        /// <returns>The quote details if found, otherwise null.</returns>
        public IQuoteReadModelSummary GetQuoteSummary(Guid tenantId, Guid quoteId)
        {
            var query = this.JoinQuoteProductOrganisation(this.dbContext.QuoteReadModels)
                .Where(rm => rm.Quote.TenantId == tenantId && rm.Quote.Id == quoteId);

            return this.ProjectSummary(query).SingleOrDefault();
        }

        /// <inheritdoc/>
        public IEnumerable<Guid> GetQuoteAggregateIdsByProductReleaseId(Guid tenantId, Guid productReleaseId, DeploymentEnvironment environment)
        {
            return this.dbContext.QuoteReadModels
                .Where(rm => rm.TenantId == tenantId
                    && rm.ProductReleaseId == productReleaseId
                    && rm.Environment == environment)
                .Select(q => q.AggregateId)
                .Distinct()
                .ToList();
        }

        /// <inheritdoc />
        public IEnumerable<Guid> GetUnassociatedQuoteAggregateIds(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            return this.dbContext.QuoteReadModels
                .Where(rm => rm.TenantId == tenantId
                    && rm.ProductId == productId
                    && rm.Environment == environment
                    && !rm.ProductReleaseId.HasValue)
                .Select(q => q.AggregateId)
                .Distinct()
                .ToList();
        }

        public IEnumerable<(Guid QuoteAggregateId, Guid QuoteId)> GetIncompleteQuotesIds(Guid tenantId, Guid productId)
        {
            using (var scope = new TransactionScope(
                    TransactionScopeOption.Required,
                    new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
            {
                var query = this.dbContext.QuoteReadModels
                    .AsNoTracking()
                    .Where(q => q.TenantId == tenantId
                        && q.ProductId == productId
                        && q.QuoteState != StandardQuoteStates.Complete
                        && q.QuoteState != StandardQuoteStates.Declined
                        && !q.IsDiscarded)
                    .Select(q => new { q.AggregateId, q.Id });
                var result = query.AsEnumerable().Select(x => (x.AggregateId, x.Id)).ToList();
                scope.Complete();
                return result;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<(Guid QuoteAggregateId, Guid QuoteId)> GetIncompleteQuoteIdsWithoutExpiryDates(Guid tenantId, Guid productId)
        {
            using (var scope = new TransactionScope(
                    TransactionScopeOption.Required,
                    new TransactionOptions { IsolationLevel = IsolationLevel.ReadUncommitted }))
            {
                var query = this.dbContext.QuoteReadModels
                    .AsNoTracking()
                    .Where(q => q.TenantId == tenantId
                        && q.ProductId == productId
                        && q.QuoteState != StandardQuoteStates.Complete
                        && q.QuoteState != StandardQuoteStates.Declined
                        && !q.IsDiscarded
                        && q.ExpiryTicksSinceEpoch == null)
                    .Select(q => new { q.AggregateId, q.Id });
                var result = query.AsEnumerable().Select(x => (x.AggregateId, x.Id)).ToList();
                scope.Complete();
                return result;
            }
        }

        /// <inheritdoc/>
        public IQuoteReadModelDetails GetQuoteDetails(Guid tenantId, Guid quoteId)
        {
            var quoteReadModels = this.dbContext.QuoteReadModels
                .Where(q => q.TenantId == tenantId && q.Id == quoteId);
            var query = this.JoinQuoteProductOrganisation(quoteReadModels);
            var queryWithPolicyAndCustomer = this.JoinPolicyCustomerRelease(query);
            IQuoteReadModelDetails quoteDetails
                = queryWithPolicyAndCustomer
                    .Select(this.DetailSelector(this.currentTicksSinceEpoch))
                    .FirstOrDefault();

            if (quoteDetails != null)
            {
                quoteDetails.Documents = this.GetAllQuoteRelatedDocuments(quoteDetails);
            }

            return quoteDetails;
        }

        /// <inheritdoc/>
        public IQuoteReadModelWithRelatedEntities GetQuoteWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, Guid quoteId, IEnumerable<string> relatedEntities)
        {
            using (MiniProfiler.Current.Step(nameof(QuoteReadModelRepository) + "." + nameof(this.GetQuoteWithRelatedEntities)))
            {
                var query = this.CreateQueryForQuoteDetailsWithRelatedEntities(tenantId, environment, relatedEntities);
                return query.FirstOrDefault(q => q.Quote.Id == quoteId);
            }
        }

        /// <inheritdoc/>
        public IQuoteReadModelWithRelatedEntities GetQuoteWithRelatedEntities(
            Guid tenantId, string quoteReference, DeploymentEnvironment? environment, IEnumerable<string> relatedEntities)
        {
            using (MiniProfiler.Current.Step(nameof(QuoteReadModelRepository) + "." + nameof(this.GetQuoteWithRelatedEntities)))
            {
                var query = this.CreateQueryForQuoteDetailsWithRelatedEntities(tenantId, environment, relatedEntities);
                return query.FirstOrDefault(q => q.Quote.QuoteNumber != null && q.Quote.QuoteNumber == quoteReference && q.Quote.Environment == environment);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<IQuoteReadModelSummary> GetQuotesByListOfProducts(
            Guid tenantId, Guid organisationId, IEnumerable<Guid> productIds, DeploymentEnvironment environment)
        {
            QuoteReadModelFilters filters = new QuoteReadModelFilters
            {
                TenantId = tenantId,
                OrganisationIds = new Guid[] { organisationId },
                Environment = environment,
            };

            var query = this.QueryQuoteReadModels(tenantId, filters)
              .Where(q => productIds.Contains(q.Quote.ProductId));

            return this.ProjectSummary(query);
        }

        /// <inheritdoc/>
        public IEnumerable<IQuoteReportItem> GetQuoteDataForReports(
            Guid tenantId,
            Guid organisationId,
            IEnumerable<Guid> productIds,
            DeploymentEnvironment environment,
            Instant fromTimestamp,
            Instant toTimestamp,
            bool includeTestData)
        {
            var query = this.dbContext.QuoteReadModels
                .Where(rm => rm.TenantId == tenantId
                    && rm.Environment == environment
                    && rm.QuoteNumber != null
                    && !rm.IsDiscarded);

            if (organisationId != default)
            {
                query = query.Where(j => j.OrganisationId == organisationId);
            }

            if (productIds != null && productIds.Any())
            {
                query = query.Where(x => productIds.Contains(x.ProductId));
            }

            if (fromTimestamp != default)
            {
                var fromTicks = fromTimestamp.ToUnixTimeTicks();
                query = query.Where(j => j.CreatedTicksSinceEpoch >= fromTicks);
            }

            if (toTimestamp != default)
            {
                var toTicks = toTimestamp.ToUnixTimeTicks();
                query = query.Where(j => j.CreatedTicksSinceEpoch <= toTicks);
            }

            if (!includeTestData)
            {
                query = query.Where(j => j.IsTestData == false);
            }

            return this.JoinQuoteProductOrganisation(query).Select(this.ReportItemSelector);
        }

        /// <inheritdoc/>
        public Guid? GetQuoteAggregateId(Guid quoteId)
        {
            var result = this.dbContext.QuoteReadModels.Where(q => q.Id == quoteId)
                .Select(q => q.AggregateId).FirstOrDefault();
            return result != default
                ? result
                : default(Guid?);
        }

        /// <inheritdoc/>
        public IQueryable<NewQuoteReadModel> GetQuotesForCustomerIdTenantIdAndOrganisationId(
            Guid tenantId, Guid organisationId, Guid customerId)
        {
            return this.GetQuotesForTenantIdAndOrganisationId(tenantId, organisationId)
                .Where(c => c.CustomerId == customerId);
        }

        /// <inheritdoc/>
        public IQueryable<NewQuoteReadModel> GetQuotesForTenantIdAndOrganisationId(Guid tenantId, Guid organisationId)
        {
            return this.dbContext.QuoteReadModels
                .Where(q => q.TenantId == tenantId)
                .Where(q => q.OrganisationId == organisationId);
        }

        /// <inheritdoc/>
        public IQueryable<NewQuoteReadModel> GetQuotes(Guid tenantId, QuoteReadModelFilters filters)
        {
            var query = this.QueryQuoteReadModels(tenantId, filters);
            return query
                .OrderByDescending(q => q.Quote.CreatedTicksSinceEpoch)
                .Select(q => q.Quote)
                .Paginate(filters);
        }

        /// <inheritdoc/>
        public IEnumerable<IQuoteReadModelWithRelatedEntities> GetQuotesWithRelatedEntities(
            Guid tenantId, QuoteReadModelFilters filters, IEnumerable<string> relatedEntities)
        {
            var quotes = this.GetQuotes(tenantId, filters);
            var query = this.CreateQueryForQuoteDetailsWithRelatedEntities(quotes, relatedEntities);
            return query.ToList();
        }

        /// <inheritdoc/>
        public IEnumerable<OrganisationMigrationQuoteReadModel> ListQuotesForOrganisationMigration(Guid tenantId)
        {
            var result = this.dbContext.QuoteReadModels.Where(p => p.TenantId == tenantId)
               .Select(q => new OrganisationMigrationQuoteReadModel
               {
                   Id = q.Id,
                   AggregateId = q.AggregateId,
                   PolicyId = q.PolicyId,
                   OrganisationId = q.OrganisationId,
               });

            return result;
        }

        /// <inheritdoc/>
        public IQueryable<QuoteReadModelWithRelatedEntities> CreateQueryForQuoteDetailsWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, IEnumerable<string> relatedEntities)
        {
            var quoteDatasource = this.dbContext.QuoteReadModels.Where(x => x.TenantId == tenantId);
            if (environment.HasValue)
            {
                quoteDatasource = quoteDatasource.Where(x => x.Environment == environment);
            }
            var query = this.CreateQueryForQuoteDetailsWithRelatedEntities(quoteDatasource, relatedEntities);
            return query;
        }

        /// <inheritdoc/>
        public List<Guid> GetQuoteIdsByEntityFilters(EntityFilters entityFilters, QuoteType? quoteType = null)
        {
            var queryable = this.dbContext.QuoteReadModels
                .Where(qrm => qrm.TenantId == entityFilters.TenantId)
                .Where(qrm => qrm.QuoteState != QuoteStatus.Nascent.ToString());

            if (quoteType.HasValue)
            {
                queryable = queryable.Where(qrm => qrm.Type == quoteType.Value);
            }

            if (entityFilters.ProductId.HasValue)
            {
                queryable = queryable.Where(qrm => qrm.ProductId == entityFilters.ProductId.Value);
            }

            if (entityFilters.OrganisationId.HasValue)
            {
                queryable = queryable.Where(qrm => qrm.OrganisationId == entityFilters.OrganisationId.Value);
            }

            if (entityFilters.Skip.HasValue && entityFilters.PageSize.HasValue)
            {
                queryable = queryable.OrderByDescending(qrm => qrm.CreatedTicksSinceEpoch).Skip(
                    entityFilters.Skip.Value).Take(entityFilters.PageSize.Value);
            }

            return queryable.Select(qrm => qrm.Id).ToList();
        }

        /// <inheritdoc/>
        public IEnumerable<NewQuoteReadModel> GetQuotesThatAreRecentlyExpired(Guid tenantId)
        {
            var currentTime = this.clock.GetCurrentInstant().ToUnixTimeTicks();
            var query = this.dbContext.QuoteReadModels
                .Where(q => q.TenantId == tenantId)
                .Where(q => q.QuoteState != StandardQuoteStates.Declined.ToLower()
                    && q.QuoteState != StandardQuoteStates.Declined
                    && q.QuoteState != StandardQuoteStates.Complete
                    && q.QuoteState != StandardQuoteStates.Complete.ToLower()
                    && q.QuoteState != StandardQuoteStates.Expired)
                .Where(q => q.ExpiryTicksSinceEpoch != null &&
                    (q.ExpiryTicksSinceEpoch < currentTime))
                .Take(40);
            var result = query.ToArray(); // can't use .AsEnumerable() here otherwise EF6 doesn't fetch all for writing
            return result;
        }

        /// <inheritdoc/>
        public NewQuoteReadModel? GetLatestQuoteOfTypeForPolicy(Guid tenantId, Guid policyId, QuoteType quoteType)
        {
            var result = this.dbContext.QuoteReadModels
                .Where(q => q.TenantId == tenantId)
                .Where(q => q.PolicyId == policyId)
                .Where(q => q.Type == quoteType)
                .OrderByDescending(q => q.CreatedTicksSinceEpoch)
                .FirstOrDefault();
            return result;
        }

        public Guid? GetQuoteIdByReferenceNumber(Guid tenantId, DeploymentEnvironment environment, string referenceNumber)
        {
            return this.dbContext.QuoteReadModels
                .Where(x => x.TenantId == tenantId && x.Environment == environment && x.QuoteNumber == referenceNumber)
                .FirstOrDefault()
                ?.Id;
        }

        private IQueryable<QuoteReadModelWithRelatedEntities> CreateQueryForQuoteDetailsWithRelatedEntities(
            IQueryable<NewQuoteReadModel> dataSource, IEnumerable<string> relatedEntities)
        {
            var query = from quote in dataSource
                        select new QuoteReadModelWithRelatedEntities
                        {
                            Quote = quote,
                            Policy = default,
                            PolicyTransactions = new PolicyTransaction[] { },
                            Customer = default,
                            Tenant = default,
                            TenantDetails = new TenantDetails[] { },
                            Organisation = default,
                            Product = default,
                            ProductDetails = new ProductDetails[] { },
                            Owner = default,
                            PolicyTransaction = default,
                            Documents = new QuoteDocumentReadModel[] { },
                            QuoteVersions = new QuoteVersionReadModel[] { },
                            Emails = new Domain.ReadWriteModel.Email.Email[] { },
                            Sms = new Sms[] { },
                            FromRelationships = new Relationship[] { },
                            ToRelationships = new Relationship[] { },
                            TextAdditionalPropertiesValues = new TextAdditionalPropertyValueReadModel[] { },
                            StructuredDataAdditionalPropertyValues = new StructuredDataAdditionalPropertyValueReadModel[] { },
                        };

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Quote.Policy))))
            {
                query = query.GroupJoin(this.dbContext.Policies, q => q.Quote.PolicyId, p => p.Id, (q, policy) => new QuoteReadModelWithRelatedEntities
                {
                    Quote = q.Quote,
                    Policy = policy.FirstOrDefault(),
                    PolicyTransactions = q.PolicyTransactions,
                    Customer = q.Customer,
                    Tenant = q.Tenant,
                    TenantDetails = q.TenantDetails,
                    Organisation = q.Organisation,
                    Product = q.Product,
                    ProductDetails = q.ProductDetails,
                    Owner = q.Owner,
                    PolicyTransaction = q.PolicyTransaction,
                    Documents = q.Documents,
                    QuoteVersions = q.QuoteVersions,
                    Emails = q.Emails,
                    Sms = q.Sms,
                    FromRelationships = q.FromRelationships,
                    ToRelationships = q.ToRelationships,
                    TextAdditionalPropertiesValues = q.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = q.StructuredDataAdditionalPropertyValues,
                });

                query = query.GroupJoin(this.dbContext.PolicyTransactions, q => q.Policy.Id, pt => pt.PolicyId, (q, pt) => new QuoteReadModelWithRelatedEntities
                {
                    Quote = q.Quote,
                    Policy = q.Policy,
                    PolicyTransactions = pt,
                    Customer = q.Customer,
                    Tenant = q.Tenant,
                    TenantDetails = q.TenantDetails,
                    Organisation = q.Organisation,
                    Product = q.Product,
                    ProductDetails = q.ProductDetails,
                    Owner = q.Owner,
                    PolicyTransaction = q.PolicyTransaction,
                    Documents = q.Documents,
                    QuoteVersions = q.QuoteVersions,
                    Emails = q.Emails,
                    Sms = q.Sms,
                    FromRelationships = q.FromRelationships,
                    ToRelationships = q.ToRelationships,
                    TextAdditionalPropertiesValues = q.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = q.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Quote.Tenant))))
            {
                query = query.Join(this.dbContext.Tenants, q => q.Quote.TenantId, t => t.Id, (q, tenant) => new QuoteReadModelWithRelatedEntities
                {
                    Quote = q.Quote,
                    Policy = q.Policy,
                    PolicyTransactions = q.PolicyTransactions,
                    Customer = q.Customer,
                    Tenant = tenant,
                    TenantDetails = tenant.DetailsCollection,
                    Organisation = q.Organisation,
                    Product = q.Product,
                    ProductDetails = q.ProductDetails,
                    Owner = q.Owner,
                    PolicyTransaction = q.PolicyTransaction,
                    Documents = q.Documents,
                    QuoteVersions = q.QuoteVersions,
                    Emails = q.Emails,
                    Sms = q.Sms,
                    FromRelationships = q.FromRelationships,
                    ToRelationships = q.ToRelationships,
                    TextAdditionalPropertiesValues = q.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = q.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Quote.Organisation))))
            {
                query = query.Join(this.dbContext.OrganisationReadModel, q => q.Quote.OrganisationId, t => t.Id, (q, organisation) => new QuoteReadModelWithRelatedEntities
                {
                    Quote = q.Quote,
                    Policy = q.Policy,
                    PolicyTransactions = q.PolicyTransactions,
                    Customer = q.Customer,
                    Tenant = q.Tenant,
                    TenantDetails = q.TenantDetails,
                    Organisation = organisation,
                    Product = q.Product,
                    ProductDetails = q.ProductDetails,
                    Owner = q.Owner,
                    PolicyTransaction = q.PolicyTransaction,
                    Documents = q.Documents,
                    QuoteVersions = q.QuoteVersions,
                    Emails = q.Emails,
                    Sms = q.Sms,
                    FromRelationships = q.FromRelationships,
                    ToRelationships = q.ToRelationships,
                    TextAdditionalPropertiesValues = q.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = q.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Quote.Product))))
            {
                query = query.Join(this.dbContext.Products.IncludeAllProperties(), q => new { tenantId = q.Quote.TenantId, productId = q.Quote.ProductId }, p => new { tenantId = p.TenantId, productId = p.Id }, (q, product) => new QuoteReadModelWithRelatedEntities
                {
                    Quote = q.Quote,
                    Policy = q.Policy,
                    PolicyTransactions = q.PolicyTransactions,
                    Customer = q.Customer,
                    Tenant = q.Tenant,
                    TenantDetails = q.TenantDetails,
                    Organisation = q.Organisation,
                    Product = product,
                    ProductDetails = product.DetailsCollection,
                    Owner = q.Owner,
                    PolicyTransaction = q.PolicyTransaction,
                    Documents = q.Documents,
                    QuoteVersions = q.QuoteVersions,
                    Emails = q.Emails,
                    Sms = q.Sms,
                    FromRelationships = q.FromRelationships,
                    ToRelationships = q.ToRelationships,
                    TextAdditionalPropertiesValues = q.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = q.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Quote.Customer))))
            {
                query = query.GroupJoin(this.dbContext.CustomerReadModels, q => q.Quote.CustomerId, c => c.Id, (q, customer) => new QuoteReadModelWithRelatedEntities
                {
                    Quote = q.Quote,
                    Policy = q.Policy,
                    PolicyTransactions = q.PolicyTransactions,
                    Customer = customer.FirstOrDefault(),
                    Tenant = q.Tenant,
                    TenantDetails = q.TenantDetails,
                    Organisation = q.Organisation,
                    Product = q.Product,
                    ProductDetails = q.ProductDetails,
                    Owner = q.Owner,
                    PolicyTransaction = q.PolicyTransaction,
                    Documents = q.Documents,
                    QuoteVersions = q.QuoteVersions,
                    Emails = q.Emails,
                    Sms = q.Sms,
                    FromRelationships = q.FromRelationships,
                    ToRelationships = q.ToRelationships,
                    TextAdditionalPropertiesValues = q.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = q.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Quote.Owner))))
            {
                query = query
                    .GroupJoin(
                        this.dbContext.Users,
                        q => q.Quote.OwnerUserId,
                        c => c.Id,
                        (q, users) => new { q, users })
                    .SelectMany(
                        x => x.users.DefaultIfEmpty(),
                        (x, user) => new QuoteReadModelWithRelatedEntities
                        {
                            Quote = x.q.Quote,
                            Policy = x.q.Policy,
                            PolicyTransactions = x.q.PolicyTransactions,
                            Customer = x.q.Customer,
                            Tenant = x.q.Tenant,
                            TenantDetails = x.q.TenantDetails,
                            Organisation = x.q.Organisation,
                            Product = x.q.Product,
                            ProductDetails = x.q.ProductDetails,
                            Owner = x.q.Quote.OwnerUserId != null ? user : null,
                            PolicyTransaction = x.q.PolicyTransaction,
                            Documents = x.q.Documents,
                            QuoteVersions = x.q.QuoteVersions,
                            Emails = x.q.Emails,
                            Sms = x.q.Sms,
                            FromRelationships = x.q.FromRelationships,
                            ToRelationships = x.q.ToRelationships,
                            TextAdditionalPropertiesValues = x.q.TextAdditionalPropertiesValues,
                            StructuredDataAdditionalPropertyValues = x.q.StructuredDataAdditionalPropertyValues,
                        });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Quote.PolicyTransaction))))
            {
                // Left Join
                query = query.GroupJoin(this.dbContext.PolicyTransactions, q => q.Quote.PolicyTransactionId, c => c.Id, (q, policyTransaction) => new QuoteReadModelWithRelatedEntities
                {
                    Quote = q.Quote,
                    Policy = q.Policy,
                    PolicyTransactions = q.PolicyTransactions,
                    Customer = q.Customer,
                    Tenant = q.Tenant,
                    TenantDetails = q.TenantDetails,
                    Organisation = q.Organisation,
                    Product = q.Product,
                    ProductDetails = q.ProductDetails,
                    Owner = q.Owner,
                    PolicyTransaction = policyTransaction.FirstOrDefault(),
                    Documents = q.Documents,
                    QuoteVersions = q.QuoteVersions,
                    Emails = q.Emails,
                    Sms = q.Sms,
                    FromRelationships = q.FromRelationships,
                    ToRelationships = q.ToRelationships,
                    TextAdditionalPropertiesValues = q.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = q.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Quote.QuoteVersions))))
            {
                query = query.GroupJoin(this.dbContext.QuoteVersions, q => q.Quote.Id, c => c.QuoteId, (q, quoteVersions) => new QuoteReadModelWithRelatedEntities
                {
                    Quote = q.Quote,
                    Policy = q.Policy,
                    PolicyTransactions = q.PolicyTransactions,
                    Customer = q.Customer,
                    Tenant = q.Tenant,
                    TenantDetails = q.TenantDetails,
                    Organisation = q.Organisation,
                    Product = q.Product,
                    ProductDetails = q.ProductDetails,
                    Owner = q.Owner,
                    PolicyTransaction = q.PolicyTransaction,
                    Documents = q.Documents,
                    QuoteVersions = quoteVersions,
                    Emails = q.Emails,
                    Sms = q.Sms,
                    FromRelationships = q.FromRelationships,
                    ToRelationships = q.ToRelationships,
                    TextAdditionalPropertiesValues = q.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = q.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Quote.Documents))))
            {
                query = query.GroupJoin(this.dbContext.QuoteDocuments, q => q.Quote.Id, c => c.QuoteOrPolicyTransactionId, (q, documents) => new QuoteReadModelWithRelatedEntities
                {
                    Quote = q.Quote,
                    Policy = q.Policy,
                    PolicyTransactions = q.PolicyTransactions,
                    Customer = q.Customer,
                    Tenant = q.Tenant,
                    TenantDetails = q.TenantDetails,
                    Organisation = q.Organisation,
                    Product = q.Product,
                    ProductDetails = q.ProductDetails,
                    Owner = q.Owner,
                    PolicyTransaction = q.PolicyTransaction,
                    Documents = documents,
                    QuoteVersions = q.QuoteVersions,
                    Emails = q.Emails,
                    Sms = q.Sms,
                    FromRelationships = q.FromRelationships,
                    ToRelationships = q.ToRelationships,
                    TextAdditionalPropertiesValues = q.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = q.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Quote.Messages))))
            {
                var emailQuery = from email in this.dbContext.Emails
                                 join relationship in this.dbContext.Relationships on new { EmailId = email.Id, Type = RelationshipType.QuoteMessage, FromEntityType = EntityType.Quote } equals new { EmailId = relationship.ToEntityId, relationship.Type, relationship.FromEntityType }
                                 select new
                                 {
                                     Email = email,
                                     RelationShip = relationship,
                                 };

                query = query.GroupJoin(emailQuery, q => q.Quote.Id, c => c.RelationShip.FromEntityId, (q, emails) => new QuoteReadModelWithRelatedEntities
                {
                    Quote = q.Quote,
                    Policy = q.Policy,
                    PolicyTransactions = q.PolicyTransactions,
                    Customer = q.Customer,
                    Tenant = q.Tenant,
                    TenantDetails = q.TenantDetails,
                    Organisation = q.Organisation,
                    Product = q.Product,
                    ProductDetails = q.ProductDetails,
                    Owner = q.Owner,
                    PolicyTransaction = q.PolicyTransaction,
                    Documents = q.Documents,
                    QuoteVersions = q.QuoteVersions,
                    Emails = emails.Select(c => c.Email),
                    Sms = q.Sms,
                    FromRelationships = q.FromRelationships,
                    ToRelationships = q.ToRelationships,
                    TextAdditionalPropertiesValues = q.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = q.StructuredDataAdditionalPropertyValues,
                });

                var smsQuery = from sms in this.dbContext.Sms
                               join relationship in this.dbContext.Relationships on new { SmsId = sms.Id, Type = RelationshipType.QuoteMessage, FromEntityType = EntityType.Quote } equals new { SmsId = relationship.ToEntityId, relationship.Type, relationship.FromEntityType }
                               select new
                               {
                                   Sms = sms,
                                   RelationShip = relationship,
                               };

                query = query.GroupJoin(smsQuery, q => q.Quote.Id, c => c.RelationShip.FromEntityId, (q, sms) => new QuoteReadModelWithRelatedEntities
                {
                    Quote = q.Quote,
                    Policy = q.Policy,
                    PolicyTransactions = q.PolicyTransactions,
                    Customer = q.Customer,
                    Tenant = q.Tenant,
                    TenantDetails = q.TenantDetails,
                    Organisation = q.Organisation,
                    Product = q.Product,
                    ProductDetails = q.ProductDetails,
                    Owner = q.Owner,
                    PolicyTransaction = q.PolicyTransaction,
                    Documents = q.Documents,
                    QuoteVersions = q.QuoteVersions,
                    Emails = q.Emails,
                    Sms = sms.Select(s => s.Sms),
                    FromRelationships = q.FromRelationships,
                    ToRelationships = q.ToRelationships,
                    TextAdditionalPropertiesValues = q.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = q.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Quote.Relationships))))
            {
                query = query.GroupJoin(this.dbContext.Relationships, q => q.Quote.Id, r => r.FromEntityId, (q, relationships) => new QuoteReadModelWithRelatedEntities
                {
                    Quote = q.Quote,
                    Policy = q.Policy,
                    PolicyTransactions = q.PolicyTransactions,
                    Customer = q.Customer,
                    Tenant = q.Tenant,
                    TenantDetails = q.TenantDetails,
                    Organisation = q.Organisation,
                    Product = q.Product,
                    ProductDetails = q.ProductDetails,
                    Owner = q.Owner,
                    PolicyTransaction = q.PolicyTransaction,
                    Documents = q.Documents,
                    QuoteVersions = q.QuoteVersions,
                    Emails = q.Emails,
                    Sms = q.Sms,
                    FromRelationships = relationships,
                    ToRelationships = q.ToRelationships,
                    TextAdditionalPropertiesValues = q.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = q.StructuredDataAdditionalPropertyValues,
                });

                query = query.GroupJoin(this.dbContext.Relationships, e => e.Quote.Id, r => r.ToEntityId, (q, relationships) => new QuoteReadModelWithRelatedEntities
                {
                    Quote = q.Quote,
                    Policy = q.Policy,
                    PolicyTransactions = q.PolicyTransactions,
                    Customer = q.Customer,
                    Tenant = q.Tenant,
                    TenantDetails = q.TenantDetails,
                    Organisation = q.Organisation,
                    Product = q.Product,
                    ProductDetails = q.ProductDetails,
                    Owner = q.Owner,
                    PolicyTransaction = q.PolicyTransaction,
                    Documents = q.Documents,
                    QuoteVersions = q.QuoteVersions,
                    Emails = q.Emails,
                    Sms = q.Sms,
                    FromRelationships = q.FromRelationships,
                    ToRelationships = relationships,
                    TextAdditionalPropertiesValues = q.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = q.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Quote.AdditionalProperties))))
            {
                query = query.GroupJoin(
                    this.dbContext.TextAdditionalPropertValues.IncludeAllProperties(),
                    q => q.Quote.Id,
                    apv => apv.EntityId,
                    (q, apv) => new QuoteReadModelWithRelatedEntities
                    {
                        Quote = q.Quote,
                        Policy = q.Policy,
                        PolicyTransactions = q.PolicyTransactions,
                        Customer = q.Customer,
                        Tenant = q.Tenant,
                        TenantDetails = q.TenantDetails,
                        Organisation = q.Organisation,
                        Product = q.Product,
                        ProductDetails = q.ProductDetails,
                        Owner = q.Owner,
                        PolicyTransaction = q.PolicyTransaction,
                        Documents = q.Documents,
                        QuoteVersions = q.QuoteVersions,
                        Emails = q.Emails,
                        Sms = q.Sms,
                        FromRelationships = q.FromRelationships,
                        ToRelationships = q.ToRelationships,
                        TextAdditionalPropertiesValues = (IEnumerable<TextAdditionalPropertyValueReadModel>)apv
                            .Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                        StructuredDataAdditionalPropertyValues = q.StructuredDataAdditionalPropertyValues,
                    })
                    .GroupJoin(
                    this.dbContext.StructuredDataAdditionalPropertyValues.IncludeAllProperties(),
                    q => q.Quote.Id,
                    apv => apv.EntityId,
                    (q, apv) => new QuoteReadModelWithRelatedEntities
                    {
                        Quote = q.Quote,
                        Policy = q.Policy,
                        PolicyTransactions = q.PolicyTransactions,
                        Customer = q.Customer,
                        Tenant = q.Tenant,
                        TenantDetails = q.TenantDetails,
                        Organisation = q.Organisation,
                        Product = q.Product,
                        ProductDetails = q.ProductDetails,
                        Owner = q.Owner,
                        PolicyTransaction = q.PolicyTransaction,
                        Documents = q.Documents,
                        QuoteVersions = q.QuoteVersions,
                        Emails = q.Emails,
                        Sms = q.Sms,
                        FromRelationships = q.FromRelationships,
                        ToRelationships = q.ToRelationships,
                        TextAdditionalPropertiesValues = q.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = (IEnumerable<StructuredDataAdditionalPropertyValueReadModel>)apv
                            .Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                    });
            }

            return query;
        }

        /// <summary>
        /// Gets an expression for instantiating summaries from QuoteReadModels for use in EF projections.
        /// </summary>
        private Expression<Func<QuoteProductOrganisationReadModel, IQuoteReadModelSummary>> GetSummarySelector(long nowTicksSinceEpoch) =>
            q => new QuoteReadModelSummary
            {
                PolicyId = q.Quote.PolicyId,
                AggregateId = q.Quote.AggregateId,
                QuoteTitle = q.Quote.QuoteTitle,
                QuoteId = q.Quote.Id,
                TenantId = q.Quote.TenantId,
                OrganisationId = q.Quote.OrganisationId,
                OrganisationAlias = q.Organisation.Alias,
                CreatedTicksSinceEpoch = q.Quote.CreatedTicksSinceEpoch,
                LastModifiedTicksSinceEpoch = q.Quote.LastModifiedTicksSinceEpoch,
                ProductId = q.Quote.ProductId,
                ProductReleaseId = q.Quote.ProductReleaseId,
                OwnerUserId = q.Quote.OwnerUserId,
                CustomerId = q.Quote.CustomerId,
                CustomerFullName = q.Quote.CustomerFullName,
                CustomerPreferredName = q.Quote.CustomerPreferredName,
                QuoteNumber = q.Quote.QuoteNumber,
                IsSubmitted = q.Quote.IsSubmitted,
                IsDiscarded = q.Quote.IsDiscarded,
                IsTestData = q.Quote.IsTestData,
                SerializedLatestCalculationResult = q.Quote.SerializedLatestCalculationResult,
                QuoteType = q.Quote.Type,
                QuoteState =
                    q.Product.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().QuoteExpirySetting.Enabled &&
                    q.Quote.ExpiryTicksSinceEpoch != null &&
                    q.Quote.ExpiryTicksSinceEpoch <= nowTicksSinceEpoch &&
                    q.Quote.QuoteState != StandardQuoteStates.Declined &&
                    q.Quote.QuoteState != StandardQuoteStates.Complete ?
                        StandardQuoteStates.Expired : q.Quote.QuoteState,
                ProductAlias = q.Product.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Alias,
                ProductName = q.Product.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Name,
                PolicyNumber = q.Quote.PolicyNumber,
                InvoiceNumber = q.Quote.InvoiceNumber,
                InvoiceTicksSinceEpoch = q.Quote.InvoiceTicksSinceEpoch,
                SumbmissionTicksSinceEpoch = q.Quote.SubmissionTicksSinceEpoch,
                IsInvoiced = q.Quote.IsInvoiced,
                IsPaidFor = q.Quote.IsPaidFor,
                LatestFormData = q.Quote.LatestFormData,
                ExpiryTicksSinceEpoch = q.Quote.ExpiryTicksSinceEpoch,
                Environment = q.Quote.Environment,
            };

        /// <summary>
        /// Gets an expression for instantiating details from QuoteReadModels for use in EF projections.
        /// </summary>
        private Expression<Func<QuoteProductOrganisationPolicyCustomerReadModel, IQuoteReadModelDetails>> DetailSelector(long nowTicksSinceEpoch) =>
            q => new QuoteReadModelDetails
            {
                PolicyNumber = q.Quote.PolicyNumber,
                QuoteId = q.Quote.Id,
                PolicyId = q.Quote.PolicyId,
                PolicyOwnerUserId = q.Policy != null ? q.Policy.OwnerUserId : null,
                AggregateId = q.Quote.AggregateId,
                QuoteType = q.Quote.Type,
                TenantId = q.Quote.TenantId,
                OrganisationId = q.Quote.OrganisationId,
                OrganisationName = q.Organisation.Name,
                CreatedTicksSinceEpoch = q.Quote.CreatedTicksSinceEpoch,
                LastModifiedTicksSinceEpoch = q.Quote.LastModifiedTicksSinceEpoch,
                ProductId = q.Quote.ProductId,
                ProductReleaseId = q.Quote.ProductReleaseId,
                ProductReleaseMajorNumber = q.Release != null ? q.Release.Number : null,
                ProductReleaseMinorNumber = q.Release != null ? q.Release.MinorNumber : null,
                OwnerUserId = q.Quote.OwnerUserId,
                OwnerFullName = q.Quote.OwnerFullName,
                CustomerId = q.Quote.CustomerId,
                CustomerFullName = q.Quote.CustomerFullName,
                CustomerPreferredName = q.Quote.CustomerPreferredName,
                CustomerOwnerUserId = q.Customer != null ? q.Customer.OwnerUserId : (Guid?)null,
                QuoteNumber = q.Quote.QuoteNumber,
                CancellationEffectiveTicksSinceEpoch = q.Quote.CreatedTicksSinceEpoch,
                IsSubmitted = q.Quote.IsSubmitted,
                LatestFormData = q.Quote.LatestFormData,
                SerializedLatestCalculationResult = q.Quote.SerializedLatestCalculationResult,
                LatestCalculationResultId = q.Quote.LatestCalculationResultId,
                LatestCalculationResultFormDataId = q.Quote.LatestCalculationResultFormDataId,
                QuoteState =
                  q.Product.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().QuoteExpirySetting.Enabled &&
                    q.Quote.ExpiryTicksSinceEpoch != null &&
                    q.Quote.ExpiryTicksSinceEpoch <= nowTicksSinceEpoch &&
                    q.Quote.QuoteState != StandardQuoteStates.Declined &&
                    q.Quote.QuoteState != StandardQuoteStates.Complete ?
                        StandardQuoteStates.Expired : q.Quote.QuoteState,
                Environment = q.Quote.Environment,
                ProductAlias = q.Product.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Alias,
                ProductName = q.Product.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Name,
                Documents = this.dbContext.QuoteDocuments.Where(qd => qd.PolicyId == q.Quote.AggregateId).ToList(),
                InvoiceNumber = q.Quote.InvoiceNumber,
                InvoiceTicksSinceEpoch = q.Quote.InvoiceTicksSinceEpoch,
                SumbmissionTicksSinceEpoch = q.Quote.SubmissionTicksSinceEpoch,
                IsInvoiced = q.Quote.IsInvoiced,
                IsPaidFor = q.Quote.IsPaidFor,
                IsDiscarded = q.Quote.IsDiscarded,
                IsTestData = q.Quote.IsTestData,
                ExpiryTicksSinceEpoch = q.Quote.ExpiryTicksSinceEpoch,
                PolicyExpiryTicksSinceEpoch = q.Quote.PolicyExpiryTicksSinceEpoch,
                PolicyTransactionEffectiveTicksSinceEpoch = q.Quote.PolicyTransactionEffectiveTicksSinceEpoch,
                PolicyInceptionTicksSinceEpoch = q.Quote.Type == QuoteType.NewBusiness
                    ? q.Quote.PolicyTransactionEffectiveTicksSinceEpoch
                    : q.Policy.InceptionTicksSinceEpoch,
                LastModifiedByUserTicksSinceEpoch = q.Quote.LastModifiedByUserTicksSinceEpoch,
            };

        private IQueryable<QuoteProductOrganisationReadModel> JoinQuoteProductOrganisation(
            IQueryable<NewQuoteReadModel> query)
        {
            this.currentTicksSinceEpoch = this.clock.Now().ToUnixTimeTicks();

            var quoteProductQuery = query.Join(
                    this.dbContext.Products.Include(p => p.DetailsCollection),
                    quote => new { productId = quote.ProductId, tenantId = quote.TenantId },
                    product => new { productId = product.Id, tenantId = product.TenantId },
                    (quote, product) => new QuoteProductReadModel
                    {
                        Quote = quote,
                        Product = product,
                    });

            var quoteProductOrganisationQuery = quoteProductQuery.Join(
                    this.dbContext.OrganisationReadModel,
                    policyAndQuoteAndProduct => new { organisationId = policyAndQuoteAndProduct.Quote.OrganisationId },
                    organisation => new { organisationId = organisation.Id },
                    (quoteAndProduct, organisation) => new QuoteProductOrganisationReadModel
                    {
                        Quote = quoteAndProduct.Quote,
                        Product = quoteAndProduct.Product,
                        Organisation = organisation,
                    });

            return quoteProductOrganisationQuery;
        }

        private IQueryable<PolicyQuoteReadModel> JoinQuoteAndPolicy()
        {
            var query = from q in this.dbContext.QuoteReadModels
                        join policy in this.dbContext.Policies on q.PolicyId equals policy.Id into qpolicy
                        from p2 in qpolicy.DefaultIfEmpty()
                        select new PolicyQuoteReadModel
                        {
                            Policy = p2,
                            Quote = q,
                        };

            return query;
        }

        /// <summary>
        /// Gets query for quote from a given tenant and environment that match given filters.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>A query for quotes.</returns>
        private IQueryable<QuoteProductOrganisationReadModel> QueryQuoteReadModels(
            Guid tenantId, QuoteReadModelFilters filters)
        {
            var frontQuery = this.dbContext.QuoteReadModels
                .Where(q => q.TenantId == tenantId);

            if (filters.Environment != null)
            {
                frontQuery = frontQuery.Where(q => q.Environment == filters.Environment);
            }

            bool hasOrganisationFilter = filters.OrganisationIds != null && filters.OrganisationIds.Any();
            if (hasOrganisationFilter)
            {
                frontQuery = frontQuery.Where(q => filters.OrganisationIds.Contains(q.OrganisationId));
            }

            if (filters.OwnerUserId != null)
            {
                frontQuery = frontQuery.Where(c => c.OwnerUserId == filters.OwnerUserId);
            }

            if (filters.CustomerId.HasValue && filters.CustomerId.GetValueOrDefault() != default)
            {
                frontQuery = frontQuery.Where(q => q.CustomerId == filters.CustomerId);
            }

            if (filters.PolicyId.HasValue && filters.PolicyId.GetValueOrDefault() != default)
            {
                frontQuery = frontQuery.Where(q => q.PolicyId == filters.PolicyId);
            }

            if (filters.DateIsAfterTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                frontQuery = frontQuery.Where(ExpressionHelper.GreaterThanExpression<NewQuoteReadModel>(filters.DateFilteringPropertyName, filters.DateIsAfterTicks));
            }

            if (filters.DateIsBeforeTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                frontQuery = frontQuery.Where(ExpressionHelper.LessThanExpression<NewQuoteReadModel>(filters.DateFilteringPropertyName, filters.DateIsBeforeTicks));
            }

            var query = this.JoinQuoteProductOrganisation(frontQuery);

            // filter out discarded quotes
            query = query.Where(q => !q.Quote.IsDiscarded);

            foreach (var searchTerm in filters.SearchTerms)
            {
                query = query.Where(rm =>
                    rm.Quote.CustomerEmail.IndexOf(searchTerm) >= 0 ||
                    rm.Quote.CustomerPreferredName.IndexOf(searchTerm) >= 0 ||
                    rm.Quote.CustomerFullName.IndexOf(searchTerm) >= 0 ||
                    rm.Quote.QuoteNumber.IndexOf(searchTerm) >= 0
                    || rm.Quote.CustomerMobilePhone.IndexOf(searchTerm) >= 0
                    || rm.Quote.CustomerWorkPhone.IndexOf(searchTerm) >= 0
                    || rm.Quote.PolicyNumber.IndexOf(searchTerm) >= 0
                    || rm.Quote.InvoiceNumber.IndexOf(searchTerm) >= 0);
            }

            if (filters.Statuses.Any())
            {
                var statusPredicate = PredicateBuilder.New<QuoteProductOrganisationReadModel>(false);
                foreach (var status in filters.Statuses)
                {
                    statusPredicate = statusPredicate.Or(this.GetExpressionForStatusMatching(status));
                }

                query = query.Where(statusPredicate);
            }

            if (filters.ExcludedStatuses.Any())
            {
                var statusPredicate = PredicateBuilder.New<QuoteProductOrganisationReadModel>(true);
                foreach (var excludeStatus in filters.ExcludedStatuses)
                {
                    statusPredicate = statusPredicate.And(rm => rm.Quote.QuoteState != excludeStatus);
                }
                query = query.Where(statusPredicate);
            }

            if (filters.QuoteTypes.Any())
            {
                ExpressionStarter<QuoteProductOrganisationReadModel> quoteTypePredicate =
                    PredicateBuilder.New<QuoteProductOrganisationReadModel>(false);
                foreach (var quoteType in filters.QuoteTypes)
                {
                    quoteTypePredicate = quoteTypePredicate.Or(rm => rm.Quote.Type.ToString() == quoteType);
                }

                query = query.Where(quoteTypePredicate);
            }

            if (!filters.IncludeTestData)
            {
                query = query.Where(rm => !rm.Quote.IsTestData);
            }

            return query;
        }

        private IEnumerable<IQuoteReadModelSummary> ProjectSummary(IQueryable<QuoteProductOrganisationReadModel> query, EntityListFilters filters = null)
        {
            var currentTicksSinceEpoch = this.clock.Now().ToUnixTimeTicks();
            return query
                .Select(this.GetSummarySelector(currentTicksSinceEpoch))
                .Paginate(filters)
                .ToList();
        }

        private IQueryable<QuoteProductOrganisationPolicyCustomerReadModel> JoinPolicyCustomerRelease(IQueryable<QuoteProductOrganisationReadModel> query)
        {
            return query
                .GroupJoin(
                    this.dbContext.CustomerReadModels,
                    model => model.Quote.CustomerId,
                    customer => customer.Id,
                    (model, customer) => new
                    {
                        model.Quote,
                        model.Product,
                        model.Organisation,
                        Customer = customer.FirstOrDefault(),
                    })
                .GroupJoin(
                    this.dbContext.Policies,
                    model => model.Quote.PolicyId,
                    policy => policy.Id,
                    (model, policy) => new
                    {
                        model.Quote,
                        model.Product,
                        model.Organisation,
                        model.Customer,
                        Policy = policy.FirstOrDefault(),
                    })
                .GroupJoin(
                    this.dbContext.Releases,
                    model => model.Quote.ProductReleaseId,
                    release => release.Id,
                    (model, release) => new QuoteProductOrganisationPolicyCustomerReadModel
                    {
                        Quote = model.Quote,
                        Product = model.Product,
                        Organisation = model.Organisation,
                        Customer = model.Customer,
                        Policy = model.Policy,
                        Release = release.FirstOrDefault(),
                    });
        }

        private Expression<Func<QuoteProductOrganisationReadModel, bool>> GetExpressionForStatusMatching(string status)
        {
            return vm => (vm.Product.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().QuoteExpirySetting.Enabled &&
                     vm.Quote.ExpiryTicksSinceEpoch != null &&
                     vm.Quote.ExpiryTicksSinceEpoch <= this.currentTicksSinceEpoch &&
                     vm.Quote.QuoteState != StandardQuoteStates.Declined &&
                     vm.Quote.QuoteState != StandardQuoteStates.Complete ?
                         StandardQuoteStates.Expired : vm.Quote.QuoteState) == status;
        }

        private List<QuoteDocumentReadModel> GetAllQuoteRelatedDocuments(IQuoteReadModelDetails quoteDetails)
        {
            var selectionPredicate = PredicateBuilder.New<QuoteDocumentReadModel>(false);
            selectionPredicate.Or(m => m.QuoteOrPolicyTransactionId == quoteDetails.QuoteId);
            var resultingTransaction = this.dbContext.PolicyTransactions
                .OfType<PolicyTransaction>()
                .SingleOrDefault(t => t.QuoteId == quoteDetails.QuoteId);
            if (resultingTransaction != null)
            {
                selectionPredicate.Or(m => m.QuoteOrPolicyTransactionId == resultingTransaction.Id);
            }

            return quoteDetails.Documents
                .Where(selectionPredicate)
                .OrderByDescending(qd => qd.CreatedTicksSinceEpoch)
                .ToList();
        }

        /// <summary>
        /// Generates a query using filters provided.
        /// </summary>
        /// <param name="filters">filters for quotes.</param>
        /// <returns>List of quotes that matches the filters.</returns>
        private async Task<IEnumerable<QuoteDashboardSummaryModel>> QueryQuotesForPeriodicSummary(QuoteReadModelFilters filters, CancellationToken cancellationToken)
        {
            StringBuilder queryBuilder = new StringBuilder();
            var parameters = new DynamicParameters();
            parameters.Add("@tenantId", filters.TenantId);
            parameters.Add("@environment", filters.Environment ?? DeploymentEnvironment.Production);
            var quoteTypes = new List<int>();
            foreach (var quoteType in filters.QuoteTypes)
            {
                if (Enum.TryParse<QuoteType>(quoteType, true, out QuoteType type))
                {
                    quoteTypes.Add((int)type);
                }
            }

            parameters.Add("@quoteTypes", quoteTypes);
            queryBuilder.Append(@"
                SELECT 
                q.Id AS Id, 
                q.Type AS [Type], 
                q.QuoteState AS QuoteState, 
                q.ProductId AS ProductId, 
                q.CreatedTicksSinceEpoch AS CreatedTicksSinceEpoch, 
                q.LastModifiedTicksSinceEpoch AS LastModifiedTimeInTicksSinceEpoch,
                q.TotalPayable AS Amount
                FROM dbo.Quotes AS q
                WHERE (q.IsTestData <> 1)
                AND (q.TenantId = @tenantId) 
                AND (q.Environment = @environment)  
                AND (q.QuoteNumber IS NOT NULL) 
                AND (q.IsDiscarded <> 1) 
                AND (q.Type in @quoteTypes)");

            if (filters.DateFilteringPropertyName != null && (filters.DateIsAfterTicks.HasValue || filters.DateIsBeforeTicks.HasValue))
            {
                if (filters.DateIsAfterTicks.HasValue)
                {
                    queryBuilder.Append(@" AND (q.CreatedTicksSinceEpoch > @dateIsAfterTicks)");
                    parameters.Add("@dateIsAfterTicks", filters.DateIsAfterTicks);
                }

                if (filters.DateIsBeforeTicks.HasValue)
                {
                    queryBuilder.Append(@" AND (q.CreatedTicksSinceEpoch < @dateIsBeforeTicks)");
                    parameters.Add("@dateIsBeforeTicks", filters.DateIsBeforeTicks);
                }
            }

            var organisationCondition = string.Empty;
            var organisationConditionForRideProtect = string.Empty;
            bool hasOrganisationFilter = filters.OrganisationIds != null && filters.OrganisationIds.Any();
            if (hasOrganisationFilter)
            {
                parameters.Add("@OrganisationIds", filters.OrganisationIds);
                organisationCondition = " AND (q.OrganisationId IN @OrganisationIds)";
                organisationConditionForRideProtect = " AND (q.OrganisationId NOT IN @OrganisationIds)";
            }

            bool queryHasRideProtect = filters.IsRideProtectOrganisation;
            bool isQueryProductIdsSpecified = filters.ProductIds.Any();
            if (queryHasRideProtect)
            {
                queryHasRideProtect = (isQueryProductIdsSpecified && filters.ProductIds.Contains(filters.RideProtectProductId.Value)) || !isQueryProductIdsSpecified;
            }

            if (queryHasRideProtect)
            {
                parameters.Add("@productIdRideProtect", filters.RideProtectProductId.Value);
                var productAndOrganisationCondition = $"(q.ProductId = @productIdRideProtect {organisationConditionForRideProtect})";
                if (isQueryProductIdsSpecified)
                {
                    // quotes from ride-protect organisation of specified productIDs and ride-protect quote from any organisation
                    parameters.Add("@productIds", filters.ProductIds);
                    productAndOrganisationCondition = $"AND ({productAndOrganisationCondition} OR (q.ProductId in @productIds {organisationCondition}))";
                }
                else
                {
                    // quotes from ride-protect organisation and ride-protect quote from any organisation
                    productAndOrganisationCondition = $"AND ({productAndOrganisationCondition} {organisationCondition.Replace("AND", "OR")})";
                }

                queryBuilder.Append(productAndOrganisationCondition);
            }
            else
            {
                queryBuilder.Append(organisationCondition);
                if (isQueryProductIdsSpecified)
                {
                    parameters.Add("@productIds", filters.ProductIds);
                    queryBuilder.Append(" AND (q.ProductId in @productIds)");
                }
            }

            // filters for user permission to view quotes
            if (filters.OwnerUserId != null)
            {
                parameters.Add("@OwnerUserId", filters.OwnerUserId);
                queryBuilder.Append(" AND (q.OwnerUserId = @OwnerUserId)");
            }

            if (filters.CustomerId != null)
            {
                parameters.Add("@CustomerId", filters.CustomerId);
                queryBuilder.Append(" AND (q.CustomerId = @CustomerId)");
            }

            var sql = queryBuilder.ToString();
            sql = $"SELECT * from ({sql}) as qResult ORDER BY qResult.CreatedTicksSinceEpoch ASC";

            using (var connection = new SqlConnection(this.connection.UBind))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
                {
                    sql = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; " + sql;
                    var command = new CommandDefinition(
                        sql,
                        parameters,
                        transaction,
                        180,
                        System.Data.CommandType.Text,
                        CommandFlags.Buffered,
                        cancellationToken);
                    var result = await connection.QueryAsync<QuoteDashboardSummaryModel>(command);
                    transaction.Commit();
                    return result;
                }
            }
        }
    }
}
