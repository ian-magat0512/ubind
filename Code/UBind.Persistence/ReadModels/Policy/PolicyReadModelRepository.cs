// <copyright file="PolicyReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using Dapper;
    using LinqKit;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Search;
    using UBind.Persistence.Extensions;
    using UBind.Persistence.ReadModels.RepositoryResourceScript;
    using UBind.Persistence.Search;

    /// <inheritdoc />
    public class PolicyReadModelRepository : IPolicyReadModelRepository
    {
        private readonly IUBindDbContext dbContext;
        private readonly IClock clock;
        private readonly IConnectionConfiguration connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyReadModelRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="connection">The connection configuration.</param>
        /// <param name="clock">A clock.</param>
        public PolicyReadModelRepository(
            IUBindDbContext dbContext,
            IConnectionConfiguration connection,
            IClock clock)
        {
            this.dbContext = dbContext;
            this.connection = connection;
            this.clock = clock;
        }

        /// <summary>
        /// Gets an expression for instantiating summaries from QuoteReadModels for use in EF projections.
        /// </summary>
        private Expression<Func<PolicyAndProductAndProductFeature, IPolicyReadModelSummary>> SummarySelector =>
           (join) => new PolicyReadModelSummary
           {
               QuoteId = join.Policy.QuoteId,
               Id = join.Policy.Id,
               PolicyTitle = join.Policy.PolicyTitle,
               TenantId = join.Policy.TenantId,
               OrganisationId = join.Policy.OrganisationId,
               CreatedTicksSinceEpoch = join.Policy.CreatedTicksSinceEpoch,
               LastModifiedTicksSinceEpoch = join.Policy.LastModifiedTicksSinceEpoch,
               ProductId = join.Policy.ProductId,
               OwnerUserId = join.Policy.OwnerUserId,
               CustomerId = join.Policy.CustomerId,
               CustomerFullName = join.Policy.CustomerFullName,
               CustomerPreferredName = join.Policy.CustomerPreferredName,
               PolicyNumber = join.Policy.PolicyNumber,
               InceptionDateTimeColumn = join.Policy.InceptionDateTimeColumn,
               InceptionTicksSinceEpoch = join.Policy.InceptionTicksSinceEpoch,
               LatestPolicyPeriodStartDateTimeColumn = join.Policy.LatestPolicyPeriodStartDateTimeColumn,
               LatestPolicyPeriodStartTicksSinceEpoch = join.Policy.LatestPolicyPeriodStartTicksSinceEpoch,
               CancellationEffectiveDateTimeColumn = join.Policy.CancellationEffectiveDateTimeColumn,
               CancellationEffectiveTicksSinceEpoch = join.Policy.CancellationEffectiveTicksSinceEpoch,
               AdjustmentEffectiveDateTimeColumn = join.Policy.AdjustmentEffectiveDateTimeColumn,
               AdjustmentEffectiveTicksSinceEpoch = join.Policy.AdjustmentEffectiveTicksSinceEpoch,
               ExpiryDateTimeColumn = join.Policy.ExpiryDateTimeColumn,
               ExpiryTicksSinceEpoch = join.Policy.ExpiryTicksSinceEpoch,
               IssuedTicksSinceEpoch = join.Policy.IssuedTicksSinceEpoch,
               LatestRenewalEffectiveTicksSinceEpoch = join.Policy.LatestRenewalEffectiveTicksSinceEpoch,
               SerializedCalculationResult = join.Policy.SerializedCalculationResult,
               IsTestData = join.Policy.IsTestData,
               ProductName = join.Product != null ? join.Product.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Name : null,
               ProductFeatureSetting = join.ProductFeature != null ? join.ProductFeature : null,
               TimeZoneId = join.Policy.TimeZoneId,
           };

        /// <summary>
        /// Gets an expression for instantiating the policy transaction and quote model.
        /// </summary>
        private Expression<Func<PolicyTransaction, IEnumerable<NewQuoteReadModel>, PolicyTransactionAndQuote>> TransactionQuoteSelector =>
           (pt, q) =>
               new PolicyTransactionAndQuote()
               {
                   PolicyTransaction = pt,
                   Quote = q.FirstOrDefault(),
               };

        /// <summary>
        /// Gets an expression for instantiating summaries from QuoteReadModels for use in EF projections.
        /// </summary>
        private Expression<Func<PolicyQuoteProductCustomerOrganisationReadModel, IPolicyReadModelDetails>> DetailSelector =>
            (q) => new PolicyReadModelDetails
            {
                Id = q.Policy.Id,
                PolicyTitle = q.Policy.PolicyTitle,
                QuoteId = q.Policy.QuoteId,
                QuoteNumber = q.Quote != null ? q.Quote.QuoteNumber : null,
                QuoteOwnerUserId = q.Quote != null ? q.Quote.OwnerUserId : default,
                TenantId = q.Policy.TenantId,
                OrganisationId = q.Policy.OrganisationId,
                OrganisationName = q.Organisation.Name,
                CreatedTicksSinceEpoch = q.Policy.CreatedTicksSinceEpoch,
                LastModifiedTicksSinceEpoch = q.Policy.LastModifiedTicksSinceEpoch,
                ProductId = q.Policy.ProductId,
                OwnerUserId = q.Policy.OwnerUserId,
                OwnerFullName = q.Policy.OwnerFullName,
                CustomerPersonId = q.Policy.CustomerPersonId,
                CustomerId = q.Policy.CustomerId,
                CustomerFullName = q.Policy.CustomerFullName,
                CustomerPreferredName = q.Policy.CustomerPreferredName,
                CustomerOwnerUserId = q.Customer != null ? q.Customer.OwnerUserId : default,
                PolicyNumber = q.Policy.PolicyNumber,
                PolicyState = q.Policy.PolicyState,
                InceptionDateTimeColumn = q.Policy.InceptionDateTimeColumn,
                InceptionTicksSinceEpoch = q.Policy.InceptionTicksSinceEpoch,
                LatestPolicyPeriodStartDateTimeColumn = q.Policy.LatestPolicyPeriodStartDateTimeColumn,
                LatestPolicyPeriodStartTicksSinceEpoch = q.Policy.LatestPolicyPeriodStartTicksSinceEpoch,
                CancellationEffectiveDateTimeColumn = q.Policy.CancellationEffectiveDateTimeColumn,
                CancellationEffectiveTicksSinceEpoch = q.Policy.CancellationEffectiveTicksSinceEpoch,
                AdjustmentEffectiveDateTimeColumn = q.Policy.AdjustmentEffectiveDateTimeColumn,
                AdjustmentEffectiveTicksSinceEpoch = q.Policy.AdjustmentEffectiveTicksSinceEpoch,
                ExpiryDateTimeColumn = q.Policy.ExpiryDateTimeColumn,
                ExpiryTicksSinceEpoch = q.Policy.ExpiryTicksSinceEpoch,
                IssuedTicksSinceEpoch = q.Policy.IssuedTicksSinceEpoch,
                SerializedCalculationResult = q.Policy.SerializedCalculationResult,
                Environment = q.Policy.Environment,
                IsTestData = q.Policy.IsTestData,
                Transactions = this.dbContext.PolicyTransactions
                    .Where(t => t.PolicyId == q.Policy.Id)
                .GroupJoin(
                    this.dbContext.QuoteReadModels,
                    transaction => transaction.QuoteId,
                    quote => quote.Id,
                    this.TransactionQuoteSelector)
                    .OrderByDescending(t => t.PolicyTransaction.CreatedTicksSinceEpoch),
                Documents = this.dbContext.QuoteDocuments
                    .Where(d => d.OwnerType == DocumentOwnerType.Policy)
                    .Where(d => d.PolicyId == q.Policy.Id)
                    .OrderByDescending(c => c.CreatedTicksSinceEpoch).ThenBy(n => n.Name),
                ProductName = q.Product.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Name,
                TimeZoneId = q.Policy.TimeZoneId,
            };

        /// <inheritdoc/>
        public IEnumerable<PolicyReadModel> GetPoliciesThatHaveRecentlyBecomeActive(Guid tenantId)
        {
            var currentTime = this.clock.GetCurrentInstant().ToUnixTimeTicks();
            var query = this.dbContext.Policies
                .Where(p => p.TenantId == tenantId
                    && p.PolicyState != "Active"
                    && ((p.CancellationEffectiveTicksSinceEpoch == default || p.CancellationEffectiveTicksSinceEpoch == 0)
                        || ((p.CancellationEffectiveTicksSinceEpoch > currentTime)
                            && (p.CancellationEffectiveTicksSinceEpoch != p.InceptionTicksSinceEpoch)))
                    && (p.InceptionTicksSinceEpoch < currentTime)
                    && (p.ExpiryTicksSinceEpoch > currentTime))
                .Take(40);
            var result = query.ToArray(); // can't use .AsEnumerable() here otherwise EF6 doesn't fetch all for writing
            return result;
        }

        /// <inheritdoc/>
        public IEnumerable<PolicyReadModel> GetPoliciesThatHaveRecentlyExpired(Guid tenantId)
        {
            var currentTime = this.clock.GetCurrentInstant().ToUnixTimeTicks();
            var query = this.dbContext.Policies
                .Where(p => p.TenantId == tenantId
                    && p.PolicyState != "Expired"
                    && ((p.CancellationEffectiveTicksSinceEpoch == default || p.CancellationEffectiveTicksSinceEpoch == 0)
                        || ((p.CancellationEffectiveTicksSinceEpoch > currentTime)
                            && (p.CancellationEffectiveTicksSinceEpoch != p.InceptionTicksSinceEpoch)))
                    && (p.ExpiryTicksSinceEpoch < currentTime))
                .Take(40);
            var result = query.ToArray(); // can't use .AsEnumerable() here otherwise EF6 doesn't fetch all for writing
            return result;
        }

        /// <inheritdoc/>
        public IEnumerable<PolicyReadModel> GetPoliciesThatHaveRecentlyCancelled(Guid tenantId)
        {
            var currentTime = this.clock.GetCurrentInstant().ToUnixTimeTicks();
            var query = this.dbContext.Policies
                .Where(p => p.TenantId == tenantId
                && p.PolicyState != "Cancelled"
                && (p.CancellationEffectiveTicksSinceEpoch != default && p.CancellationEffectiveTicksSinceEpoch != 0)
                && ((p.CancellationEffectiveTicksSinceEpoch < currentTime)
                    || (p.CancellationEffectiveTicksSinceEpoch == p.InceptionTicksSinceEpoch)))
                .Take(40);
            var result = query.ToArray(); // can't use .AsEnumerable() here otherwise EF6 doesn't fetch all for writing
            return result;
        }

        /// <inheritdoc/>
        public IEnumerable<IPolicyReadModelSummary> ListPolicies(
            Guid tenantId, PolicyReadModelFilters filters)
        {
            var query = this.QueryPolicyReadModels(tenantId, filters);
            return this.GetSummaries(query, filters);
        }

        /// <inheritdoc/>
        public IEnumerable<Guid> ListPolicyIds(
            Guid tenantId,
            PolicyReadModelFilters filters)
        {
            var query = this.QueryPolicyReadModels(tenantId, filters);
            return this.GetPolicyIds(query, filters);
        }

        /// <summary>
        /// List policies for export in a given tenant and environment that match given filters.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>The set of summaries of matching policies for export.</returns>
        public IEnumerable<IPolicyReadModelSummary> ListPoliciesForExport(
            Guid tenantId, PolicyReadModelFilters filters)
        {
            return this.GetPoliciesForExport(tenantId, filters);
        }

        /// <summary>
        /// List quotes in a given tenant and evironment belonging to a given owner that match given filters.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="ownerId">The owner ID.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>The set of summaries of matching quotes.</returns>
        public IEnumerable<IPolicyReadModelSummary> ListPoliciesByOwner(
            Guid tenantId, Guid ownerId, PolicyReadModelFilters filters)
        {
            var query = this.QueryPolicyReadModels(tenantId, filters)
            .Where(policy => policy.OwnerUserId == ownerId);

            return this.GetSummaries(query, filters);
        }

        /// <inheritdoc/>
        public IEnumerable<Guid> ListPolicyIdsFromCustomer(Guid tenantId, Guid customerId, DeploymentEnvironment environment)
        {
            var query = this.dbContext.Policies.Where(p =>
                p.TenantId == tenantId
                && p.CustomerId == customerId
                && p.Environment == environment).Select(p => p.Id);
            return query.ToList();
        }

        /// <summary>
        /// List quotes in a given tenant and evironment belonging to a given customer that match given filters.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="customerId">Customer ID.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>The set of summaries of matching quotes.</returns>
        public IEnumerable<IPolicyReadModelSummary> ListPoliciesByCustomer(
            Guid tenantId, Guid customerId, PolicyReadModelFilters filters)
        {
            var query = this.QueryPolicyReadModels(tenantId, filters)
                        .Where(policy => policy.CustomerId == customerId);
            return this.GetSummaries(query, filters);
        }

        /// <inheritdoc/>
        public bool HasPoliciesForCustomer(PolicyReadModelFilters filters, IEnumerable<Guid> excludedPolicyIds)
        {
            return this.dbContext.Policies.Any(p => p.TenantId == filters.TenantId
                && (filters.OrganisationIds.Any() ? filters.OrganisationIds.Contains(p.OrganisationId) : true)
                && p.Environment == filters.Environment
                && p.CustomerId == filters.CustomerId
                && !excludedPolicyIds.Contains(p.Id));
        }

        /// <inheritdoc/>
        public bool HasPolicy(Guid policyId)
        {
            return this.dbContext.Policies.Any(p => p.Id == policyId);
        }

        /// <inheritdoc/>
        public bool HasPolicyForTenant(Guid tenantId, Guid policyId)
        {
            return this.dbContext.Policies.Any(p => p.Id == policyId
                && p.TenantId == tenantId);
        }

        /// <summary>
        /// Gets details of a given policy.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="policyId">The ID of the quote whose policy to fetch.</param>
        /// <returns>The policy details if found, otherwise null.</returns>
        public IPolicyReadModelDetails GetPolicyDetails(Guid tenantId, Guid policyId)
        {
            var query = this.dbContext.Policies
                .Where(p => p.TenantId == tenantId && p.Id == policyId);

            return this.JoinQuotePolicyProductOrganisation(query)
                    .Select(this.DetailSelector)
                    .FirstOrDefault();
        }

        /// <summary>
        /// Gets details of a given policy.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="quoteId">The ID of the quote whose policy to fetch.</param>
        /// <returns>The policy details if found, otherwise null.</returns>
        public IPolicyReadModelDetails GetPolicyTransactionDetails(Guid tenantId, Guid quoteId)
        {
            var query = this.dbContext.QuoteReadModels
                .Where(q => q.TenantId == tenantId && q.Id == quoteId)
                .Join(
                    this.dbContext.Policies,
                    quote => new { policyId = quote.PolicyId ?? Guid.Empty },
                    policy => new { policyId = policy.Id },
                    (q, p) => new
                    {
                        Policy = p,
                        Quote = q,
                    })
                .GroupJoin(
                    this.dbContext.CustomerReadModels,
                    model => model.Policy.CustomerId,
                    customer => customer.Id,
                    (model, customer) => new
                    {
                        Policy = model.Policy,
                        Quote = model.Quote,
                        Customer = customer.FirstOrDefault(),
                    })
                .Join(
                    this.dbContext.Products.Include(p => p.DetailsCollection),
                    pq => pq.Policy.ProductId,
                    prod => prod.Id,
                    (model, prod) => new PolicyQuoteCustomerProductReadModel
                    {
                        Policy = model.Policy,
                        Quote = model.Quote,
                        Customer = model.Customer,
                        Product = prod,
                    });

            var policyQuoteProductOrganisationQuery = query.Join(
                    this.dbContext.OrganisationReadModel,
                    policyAndQuoteAndProduct => new { organisationId = policyAndQuoteAndProduct.Quote.OrganisationId },
                    organisation => new { organisationId = organisation.Id },
                    (policyAndQuoteAndProduct, organisation) => new PolicyQuoteProductCustomerOrganisationReadModel
                    {
                        Policy = policyAndQuoteAndProduct.Policy,
                        Quote = policyAndQuoteAndProduct.Quote,
                        Product = policyAndQuoteAndProduct.Product,
                        Organisation = organisation,
                    });

            return policyQuoteProductOrganisationQuery
                    .Select(this.DetailSelector)
                    .FirstOrDefault();
        }

        /// <inheritdoc/>
        public PolicyReadModel GetPolicyByNumber(
            Guid tenantId, Guid productId, DeploymentEnvironment environment, string policyNumber)
        {
            return this.dbContext.Policies.FirstOrDefault(p => p.Environment == environment
                                                            && p.TenantId == tenantId
                                                            && p.ProductId == productId
                                                            && p.PolicyNumber == policyNumber);
        }

        /// <inheritdoc/>
        public Guid GetQuoteAggregateId(Guid policyId)
        {
            // TODO: add the aggregate ID to the policy read model so that policyId can be different to the Aggregate ID
            // This will be done in UB-5418.
            return policyId;
            /*
            return this.dbContext.Policies.FirstOrDefault(p => p.Id == policyId).AggregateId;
            */
        }

        /// <inheritdoc/>
        public Guid? GetQuoteId(Guid policyId)
        {
            return this.dbContext.Policies.FirstOrDefault(p => p.Id == policyId).QuoteId;
        }

        /// <inheritdoc/>
        public IPolicyReadModelWithRelatedEntities GetPolicyWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, Guid policyId, IEnumerable<string> relatedEntities)
        {
            var query = this.CreateQueryForPolicyDetailsWithRelatedEntities(tenantId, environment, relatedEntities);
            return query.FirstOrDefault(q => q.Policy.Id == policyId);
        }

        /// <inheritdoc/>
        public IPolicyReadModelWithRelatedEntities GetPolicyWithRelatedEntities(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment? environment,
            string policyNumber,
            IEnumerable<string> relatedEntities)
        {
            var query = this.CreateQueryForPolicyDetailsWithRelatedEntities(tenantId, environment, relatedEntities);

            return query
                .FirstOrDefault(q =>
                    q.Policy.PolicyNumber == policyNumber
                    && q.Policy.TenantId == tenantId
                    && q.Policy.ProductId == productId);
        }

        /// <inheritdoc/>
        public List<Guid> GetAllPolicyIdsWithQuoteByEntityFilters(EntityFilters entityFilters)
        {
            var query = this.dbContext.Policies.Where(
                pol => pol.TenantId == entityFilters.TenantId && pol.QuoteId != Guid.Empty);

            if (entityFilters.ProductId.HasValue)
            {
                query = query.Where(pol => pol.ProductId == entityFilters.ProductId.Value);
            }

            if (entityFilters.OrganisationId.HasValue)
            {
                query = query.Where(pol => pol.OrganisationId == entityFilters.OrganisationId.Value);
            }

            if (entityFilters.Skip.HasValue && entityFilters.PageSize.HasValue)
            {
                query = query.OrderByDescending(
                    po => po.CreatedTicksSinceEpoch).Skip(entityFilters.Skip.Value).Take(
                    entityFilters.PageSize.Value);
            }

            return query.Select(pol => pol.Id).ToList();
        }

        /// <inheritdoc/>
        public IEnumerable<OrganisationMigrationPolicyReadModel> ListPoliciesForOrganisationMigration(Guid tenantId)
        {
            var result = this.dbContext.Policies.Where(p => p.TenantId == tenantId)
           .Select(p => new OrganisationMigrationPolicyReadModel
           {
               Id = p.Id,
               QuoteId = p.QuoteId,
               OrganisationId = p.OrganisationId,
           });

            return result;
        }

        /// <inheritdoc/>
        public IQueryable<IPolicyReadModelWithRelatedEntities> CreateQueryForPolicyDetailsWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, IEnumerable<string> relatedProperties)
        {
            var policies = this.dbContext.Policies.Where(x => x.TenantId == tenantId);
            if (environment.HasValue)
            {
                policies = policies.Where(x => x.Environment == environment);
            }

            return this.CreateQueryForPolicyDetailsWithRelatedEntities(
                policies,
                relatedProperties);
        }

        /// <inheritdoc/>
        public PolicyReadModel GetById(Guid tenantId, Guid id)
        {
            return this.dbContext.Policies.FirstOrDefault(x => x.TenantId == tenantId && x.Id == id);
        }

        public Guid GetQuoteAggregateIdForPolicyId(Guid tenantId, Guid policyId)
        {
            // TODO: Currently the quoteAggregateId is the same as the policyId however in future we will change this
            // so they will be different
            return policyId;
        }

        /// <inheritdoc/>
        public IEnumerable<Guid> ListPolicyIdsWithoutAggregates(Guid tenantId, Guid productId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ProductId", productId);
            parameters.Add("@TenantId", tenantId);

            string sql = @"select 
                        p.Id from PolicyReadModels p
                        WHERE NOT EXISTS (SELECT 1 FROM EventRecordWithGuidIds e WHERE e.AggregateId = p.Id)
                        and p.TenantId = @TenantId
                        and p.ProductId = @ProductId";

            using (var connection = new SqlConnection(this.connection.UBind))
            {
                var policies = connection.Query<PolicyReadModel>(sql, parameters, null, true, 180, System.Data.CommandType.Text).ToList();
                return policies.Select(x => x.Id);
            }
        }

        /// <inheritdoc/>
        public int GetPolicyCountBetweenDates(
            Guid tenantId,
            DeploymentEnvironment environment,
            Instant fromDateTime,
            Instant toDateTime)
        {
            var fromDateTimeTicksSinceEpoch = fromDateTime.ToUnixTimeTicks();
            var toDateTimeTicksSinceEpoch = toDateTime.ToUnixTimeTicks();

            var query = this.dbContext.Policies.Where(
                p => p.TenantId == tenantId
            && p.Environment == environment
            && p.InceptionTicksSinceEpoch > 0
            && p.PolicyNumber != null
            && (p.LastModifiedTicksSinceEpoch >= fromDateTimeTicksSinceEpoch
                || p.LastModifiedByUserTicksSinceEpoch >= fromDateTimeTicksSinceEpoch)
            && (p.LastModifiedTicksSinceEpoch <= toDateTimeTicksSinceEpoch
                || p.LastModifiedByUserTicksSinceEpoch <= toDateTimeTicksSinceEpoch));

            return query.Count();
        }

        /// <inheritdoc/>
        public IEnumerable<IPolicySearchIndexWriteModel> GetPolicyForSearchIndexCreation(
            Guid tenantId,
            DeploymentEnvironment environment,
            EntityListFilters filters,
            long? lastUpdatedSearchIndexDateInTicks = null)
        {
            try
            {
                long lastUpdatedInTicks = lastUpdatedSearchIndexDateInTicks.HasValue ? lastUpdatedSearchIndexDateInTicks.Value : 0;

                var parameters = new DynamicParameters();
                parameters.Add("@LastModifiedTicksSinceEpoch", lastUpdatedInTicks);
                parameters.Add("@TenantId", tenantId);
                parameters.Add("@Environment", environment);
                parameters.Add("@Offset", filters.PageSize * (filters.Page - 1));
                parameters.Add("@Next", filters.PageSize);

                using (var connection = new SqlConnection(this.connection.UBind))
                {
                    List<PolicySearchIndexWriteModel> writeModels = new List<PolicySearchIndexWriteModel>();

                    void QueryWriteModel()
                    {
                        var policyList = connection.Query<PolicySearchIndexWriteModel, PolicyTransactionSearchIndexWriteModel, PolicySearchIndexWriteModel>(
                            PolicyRepositoryResourceScript.GetPolicyReadModelAndPolicyTransaction,
                            (policy, policyTransaction) =>
                            {
                                policy.PolicyTransactionModel = new List<IPolicyTransactionSearchIndexWriteModel>()
                                {
                                    policyTransaction,
                                };
                                return policy;
                            },
                            parameters,
                            null,
                            true,
                            splitOn: "PolicyTransactionId",
                            180,
                            System.Data.CommandType.Text).ToList();

                        writeModels = policyList.GroupBy(p => p.Id).Select(g =>
                        {
                            var policy = g.First();
                            policy.PolicyTransactionModel = g.Select(p => p.PolicyTransactionModel.Single()).ToList();
                            return policy;
                        }).ToList();
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

                throw new LuceneIndexingException(
                    $"Error in getting the Policy from Database. {innerExceptionMessage} Tenant : {tenantId}, "
                    + $"Environment : {environment}, LastUpdatedInTicks: {lastUpdatedSearchIndexDateInTicks}, "
                    + $"Details: {ex.Message}.", ex);
            }
        }

        /// <summary>
        /// Gets query for quote from a given tenant and environment that match given filters.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>A query for quotes.</returns>
        public IQueryable<PolicyReadModel> QueryPolicyReadModels(
            Guid tenantId, PolicyReadModelFilters filters)
        {
            var query = this.dbContext.Policies
                .Where(q => q.TenantId == tenantId);

            // make sure it actually has a policy. There was some code which created policy read model records
            // at the time of quote creation which created thousands of unnecessary records.
            // If we don't filter them out, they'll have null dates, and then our projection will cause an
            // exception. We know it really has a policy if it has an inception time.
            query = query.Where(p => p.InceptionTicksSinceEpoch > 0);

            if (filters.Environment != null)
            {
                query = query.Where(q => q.Environment == filters.Environment);
            }

            bool hasOrganisationFilter = filters.OrganisationIds != null && filters.OrganisationIds.Any();
            if (hasOrganisationFilter)
            {
                query = query.Where(q => filters.OrganisationIds.Contains(q.OrganisationId));
            }

            if (filters.ProductId != null)
            {
                query = query.Where(q => q.ProductId == filters.ProductId.Value);
            }

            if (!string.IsNullOrEmpty(filters.PolicyNumber))
            {
                query = query.Where(q => q.PolicyNumber == filters.PolicyNumber);
            }

            if (filters.PolicyId != null)
            {
                query = query.Where(q => q.Id == filters.PolicyId);
            }

            if (filters.SearchTerms.Any())
            {
                var searchExpression = PredicateBuilder.New<PolicyReadModel>(false);
                foreach (var searchTerm in filters.SearchTerms)
                {
                    searchExpression.Or(q => q.CustomerEmail.IndexOf(searchTerm) >= 0 ||
                        q.CustomerPreferredName.IndexOf(searchTerm) >= 0 ||
                        q.CustomerFullName.IndexOf(searchTerm) >= 0 ||
                        q.PolicyNumber.IndexOf(searchTerm) >= 0);
                }

                query = query.Where(searchExpression);
            }

            if (filters.DateIsAfterTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(ExpressionHelper.GreaterThanExpression<PolicyReadModel>(filters.DateFilteringPropertyName, filters.DateIsAfterTicks));
            }

            if (filters.DateIsBeforeTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(ExpressionHelper.LessThanExpression<PolicyReadModel>(filters.DateFilteringPropertyName, filters.DateIsBeforeTicks));
            }

            if (filters.Statuses.Any())
            {
                var currentTicksSinceEpoch = this.clock.GetCurrentInstant().ToUnixTimeTicks();
                var statusPredicate = PredicateBuilder.New<PolicyReadModel>(false);
                foreach (var status in filters.Statuses.Where(x => x != null).Select(s => s.ToEnumOrThrow<PolicyStatus>()))
                {
                    statusPredicate = statusPredicate.Or(this.GetExpressionForStatusMatching(status, currentTicksSinceEpoch));
                }

                query = query.Where(statusPredicate);
            }

            if (!filters.IncludeTestData)
            {
                query = query.Where(q => q.IsTestData == false);
            }

            if (filters.OwnerUserId != null)
            {
                query = query.Where(q => q.OwnerUserId == filters.OwnerUserId);
            }

            if (filters.CustomerId.HasValue && filters.CustomerId.GetValueOrDefault() != default)
            {
                query = query.Where(q => q.CustomerId == filters.CustomerId);
            }

            if (!string.IsNullOrEmpty(filters.SortBy))
            {
                query = query.Order(filters.SortBy, filters.SortOrder);
            }

            return query.Paginate(filters);
        }

        /// <inheritdoc/>
        public IEnumerable<IPolicyReadModelWithRelatedEntities> GetPoliciesWithRelatedEntities(
            Guid tenantId, PolicyReadModelFilters filters, IEnumerable<string> relatedEntities)
        {
            var policies = this.QueryPolicyReadModels(tenantId, filters);

            return this.CreateQueryForPolicyDetailsWithRelatedEntities(policies, relatedEntities);
        }

        /// <inheritdoc/>
        public Guid? GetProductReleaseIdForLatestPolicyPeriodTransaction(
            Guid tenantId, Guid productId, Guid policyId, DeploymentEnvironment environment)
        {
            return this.dbContext.PolicyTransactions
                .Where(pt => pt.TenantId == tenantId
                    && pt.ProductId == productId
                    && pt.PolicyId == policyId
                    && pt.Environment == environment
                    && (pt is NewBusinessTransaction || pt is RenewalTransaction))
                    .OrderByDescending(pt => pt.CreatedTicksSinceEpoch)
                    .Select(pt => pt.ProductReleaseId)
                    .FirstOrDefault();
        }

        /// <inheritdoc/>
        public Guid? GetProductReleaseId(Guid tenantId, Guid policyId)
        {
            return this.dbContext.PolicyTransactions
                .Where(pt => pt.TenantId == tenantId && pt.PolicyId == policyId
                    && (pt is NewBusinessTransaction || pt is RenewalTransaction))
                .OrderByDescending(pt => pt.CreatedTicksSinceEpoch)
                .Select(pt => pt.ProductReleaseId)
                .FirstOrDefault();
        }

        /// <inheritdoc/>
        public IEnumerable<Guid> GetPolicyTransactionAggregateIdsByProductReleaseId(
            Guid tenantId, Guid productReleaseId, DeploymentEnvironment environment)
        {
            return this.dbContext.PolicyTransactions
                .Where(pt => pt.TenantId == tenantId
                        && pt.ProductReleaseId == productReleaseId
                        && pt.Environment == environment)
                .Select(pt => pt.PolicyId)
                .Distinct()
                .ToList();
        }

        /// <inheritdoc/>
        public IEnumerable<Guid> GetUnassociatedPolicyTransactionAggregateIds(
            Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            return this.dbContext.PolicyTransactions
                .Where(pt => pt.TenantId == tenantId
                        && pt.ProductId == productId
                        && pt.Environment == environment
                        && !pt.ProductReleaseId.HasValue)
                .Select(pt => pt.PolicyId)
                .Distinct()
                .ToList();
        }

        private IQueryable<IPolicyReadModelWithRelatedEntities> CreateQueryForPolicyDetailsWithRelatedEntities(
        IQueryable<PolicyReadModel> dataSource, IEnumerable<string> relatedProperties)
        {
            var query = from policy in dataSource
                        select new PolicyReadModelWithRelatedEntities
                        {
                            Policy = policy,
                            Customer = default,
                            Tenant = default,
                            Organisation = default,
                            TenantDetails = new TenantDetails[] { },
                            Product = default,
                            ProductDetails = new ProductDetails[] { },
                            Owner = default,
                            QuoteDocuments = new QuoteDocumentReadModel[] { },
                            ClaimDocuments = new ClaimAttachmentReadModel[] { },
                            Emails = new UBind.Domain.ReadWriteModel.Email.Email[] { },
                            Quotes = new NewQuoteReadModel[] { },
                            Claims = new ClaimReadModel[] { },
                            PolicyTransactions = new PolicyTransaction[] { },
                            Sms = new Sms[] { },
                            FromRelationships = new Relationship[] { },
                            ToRelationships = new Relationship[] { },
                            TextAdditionalPropertiesValues = new TextAdditionalPropertyValueReadModel[] { },
                            StructuredDataAdditionalPropertyValues = new StructuredDataAdditionalPropertyValueReadModel[] { },
                        };

            var transactionRequired = new List<string>
            {
                nameof(Domain.SerialisedEntitySchemaObject.Policy.PolicyTransactions).ToUpper(),
                nameof(Domain.SerialisedEntitySchemaObject.Policy.FormData).ToUpper(),
                nameof(Domain.SerialisedEntitySchemaObject.Policy.FormDataFormatted).ToUpper(),
            };

            if (relatedProperties.Any(a => transactionRequired.Contains(a.ToUpper())))
            {
                query = query.GroupJoin(this.dbContext.PolicyTransactions, p => p.Policy.Id, c => c.PolicyId, (p, policyTransactions) => new PolicyReadModelWithRelatedEntities
                {
                    Policy = p.Policy,
                    Customer = p.Customer,
                    Tenant = p.Tenant,
                    Organisation = p.Organisation,
                    TenantDetails = p.TenantDetails,
                    Product = p.Product,
                    ProductDetails = p.ProductDetails,
                    Owner = p.Owner,
                    QuoteDocuments = p.QuoteDocuments,
                    ClaimDocuments = p.ClaimDocuments,
                    Emails = p.Emails,
                    Quotes = p.Quotes,
                    Claims = p.Claims,
                    PolicyTransactions = policyTransactions,
                    Sms = p.Sms,
                    FromRelationships = p.FromRelationships,
                    ToRelationships = p.ToRelationships,
                    TextAdditionalPropertiesValues = p.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = p.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedProperties.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Policy.Tenant))))
            {
                query = query.Join(this.dbContext.Tenants, p => p.Policy.TenantId, t => t.Id, (p, tenant) => new PolicyReadModelWithRelatedEntities
                {
                    Policy = p.Policy,
                    Customer = p.Customer,
                    Tenant = tenant,
                    Organisation = p.Organisation,
                    TenantDetails = tenant.DetailsCollection,
                    Product = p.Product,
                    ProductDetails = p.ProductDetails,
                    Owner = p.Owner,
                    QuoteDocuments = p.QuoteDocuments,
                    ClaimDocuments = p.ClaimDocuments,
                    Emails = p.Emails,
                    Quotes = p.Quotes,
                    Claims = p.Claims,
                    PolicyTransactions = p.PolicyTransactions,
                    Sms = p.Sms,
                    FromRelationships = p.FromRelationships,
                    ToRelationships = p.ToRelationships,
                    TextAdditionalPropertiesValues = p.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = p.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedProperties.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Policy.Organisation))))
            {
                query = query.Join(this.dbContext.OrganisationReadModel, p => p.Policy.OrganisationId, t => t.Id, (p, organisation) => new PolicyReadModelWithRelatedEntities
                {
                    Policy = p.Policy,
                    Customer = p.Customer,
                    Tenant = p.Tenant,
                    Organisation = organisation,
                    TenantDetails = p.TenantDetails,
                    Product = p.Product,
                    ProductDetails = p.ProductDetails,
                    Owner = p.Owner,
                    QuoteDocuments = p.QuoteDocuments,
                    ClaimDocuments = p.ClaimDocuments,
                    Emails = p.Emails,
                    Quotes = p.Quotes,
                    Claims = p.Claims,
                    PolicyTransactions = p.PolicyTransactions,
                    Sms = p.Sms,
                    FromRelationships = p.FromRelationships,
                    ToRelationships = p.ToRelationships,
                    TextAdditionalPropertiesValues = p.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = p.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedProperties.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Policy.Product))))
            {
                query = query.Join(this.dbContext.Products.IncludeAllProperties(), p => new { tenantId = p.Policy.TenantId, productId = p.Policy.ProductId }, t => new { tenantId = t.TenantId, productId = t.Id }, (p, product) => new PolicyReadModelWithRelatedEntities
                {
                    Policy = p.Policy,
                    Customer = p.Customer,
                    Tenant = p.Tenant,
                    Organisation = p.Organisation,
                    TenantDetails = p.TenantDetails,
                    Product = product,
                    ProductDetails = product.DetailsCollection,
                    Owner = p.Owner,
                    QuoteDocuments = p.QuoteDocuments,
                    ClaimDocuments = p.ClaimDocuments,
                    Emails = p.Emails,
                    Quotes = p.Quotes,
                    Claims = p.Claims,
                    PolicyTransactions = p.PolicyTransactions,
                    Sms = p.Sms,
                    FromRelationships = p.FromRelationships,
                    ToRelationships = p.ToRelationships,
                    TextAdditionalPropertiesValues = p.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = p.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedProperties.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Policy.Customer))))
            {
                query = query
                .GroupJoin(
                    this.dbContext.CustomerReadModels,
                            p => p.Policy.CustomerId,
                            c => c.Id,
                            (p, customers) => new { Policy = p, Customers = customers })
                .Select(p => new PolicyReadModelWithRelatedEntities
                {
                    Policy = p.Policy.Policy,
                    Customer = p.Customers.FirstOrDefault(),
                    Tenant = p.Policy.Tenant,
                    Organisation = p.Policy.Organisation,
                    TenantDetails = p.Policy.TenantDetails,
                    Product = p.Policy.Product,
                    ProductDetails = p.Policy.ProductDetails,
                    Owner = p.Policy.Owner,
                    QuoteDocuments = p.Policy.QuoteDocuments,
                    ClaimDocuments = p.Policy.ClaimDocuments,
                    Emails = p.Policy.Emails,
                    Quotes = p.Policy.Quotes,
                    Claims = p.Policy.Claims,
                    PolicyTransactions = p.Policy.PolicyTransactions,
                    Sms = p.Policy.Sms,
                    FromRelationships = p.Policy.FromRelationships,
                    ToRelationships = p.Policy.ToRelationships,
                    TextAdditionalPropertiesValues = p.Policy.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = p.Policy.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedProperties.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Policy.Owner))))
            {
                query = query.GroupJoin(this.dbContext.Users, p => p.Policy.OwnerUserId, u => u.Id, (p, users) => new { p, users })
                    .SelectMany(
                        x => x.users.DefaultIfEmpty(),
                        (x, user) => new PolicyReadModelWithRelatedEntities
                        {
                            Policy = x.p.Policy,
                            Customer = x.p.Customer,
                            Tenant = x.p.Tenant,
                            Organisation = x.p.Organisation,
                            TenantDetails = x.p.TenantDetails,
                            Product = x.p.Product,
                            ProductDetails = x.p.ProductDetails,
                            Owner = x.p.Policy.OwnerUserId != null ? user : null,
                            QuoteDocuments = x.p.QuoteDocuments,
                            ClaimDocuments = x.p.ClaimDocuments,
                            Emails = x.p.Emails,
                            Quotes = x.p.Quotes,
                            Claims = x.p.Claims,
                            PolicyTransactions = x.p.PolicyTransactions,
                            Sms = x.p.Sms,
                            FromRelationships = x.p.FromRelationships,
                            ToRelationships = x.p.ToRelationships,
                            TextAdditionalPropertiesValues = x.p.TextAdditionalPropertiesValues,
                            StructuredDataAdditionalPropertyValues = x.p.StructuredDataAdditionalPropertyValues,
                        });
            }

            if (relatedProperties.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Policy.Documents))))
            {
                query = query.GroupJoin(this.dbContext.QuoteDocuments, p => p.Policy.Id, c => c.PolicyId, (p, documents) => new PolicyReadModelWithRelatedEntities
                {
                    Policy = p.Policy,
                    Customer = p.Customer,
                    Tenant = p.Tenant,
                    Organisation = p.Organisation,
                    TenantDetails = p.TenantDetails,
                    Product = p.Product,
                    ProductDetails = p.ProductDetails,
                    Owner = p.Owner,
                    QuoteDocuments = documents,
                    ClaimDocuments = p.ClaimDocuments,
                    Emails = p.Emails,
                    Quotes = p.Quotes,
                    Claims = p.Claims,
                    PolicyTransactions = p.PolicyTransactions,
                    Sms = p.Sms,
                    FromRelationships = p.FromRelationships,
                    ToRelationships = p.ToRelationships,
                    TextAdditionalPropertiesValues = p.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = p.StructuredDataAdditionalPropertyValues,
                });

                query = query.GroupJoin(this.dbContext.ClaimAttachment, p => p.Customer.Id, c => c.CustomerId, (p, documents) => new PolicyReadModelWithRelatedEntities
                {
                    Policy = p.Policy,
                    Customer = p.Customer,
                    Tenant = p.Tenant,
                    Organisation = p.Organisation,
                    TenantDetails = p.TenantDetails,
                    Product = p.Product,
                    ProductDetails = p.ProductDetails,
                    Owner = p.Owner,
                    QuoteDocuments = p.QuoteDocuments,
                    ClaimDocuments = documents,
                    Emails = p.Emails,
                    Quotes = p.Quotes,
                    Claims = p.Claims,
                    PolicyTransactions = p.PolicyTransactions,
                    Sms = p.Sms,
                    FromRelationships = p.FromRelationships,
                    ToRelationships = p.ToRelationships,
                    TextAdditionalPropertiesValues = p.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = p.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedProperties.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Policy.Messages))))
            {
                var emailQuery = from email in this.dbContext.Emails
                                 join relationship in this.dbContext.Relationships on new { EmailId = email.Id, Type = RelationshipType.PolicyMessage, FromEntityType = EntityType.Policy } equals new { EmailId = relationship.ToEntityId, Type = relationship.Type, FromEntityType = relationship.FromEntityType }
                                 select new
                                 {
                                     Email = email,
                                     RelationShip = relationship,
                                 };

                query = query.GroupJoin(emailQuery, p => p.Policy.Id, c => c.RelationShip.FromEntityId, (p, emails) => new PolicyReadModelWithRelatedEntities
                {
                    Policy = p.Policy,
                    Customer = p.Customer,
                    Tenant = p.Tenant,
                    Organisation = p.Organisation,
                    TenantDetails = p.TenantDetails,
                    Product = p.Product,
                    ProductDetails = p.ProductDetails,
                    Owner = p.Owner,
                    QuoteDocuments = p.QuoteDocuments,
                    ClaimDocuments = p.ClaimDocuments,
                    Emails = emails.Select(c => c.Email),
                    Quotes = p.Quotes,
                    Claims = p.Claims,
                    PolicyTransactions = p.PolicyTransactions,
                    Sms = p.Sms,
                    FromRelationships = p.FromRelationships,
                    ToRelationships = p.ToRelationships,
                    TextAdditionalPropertiesValues = p.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = p.StructuredDataAdditionalPropertyValues,
                });

                var smsQuery = from sms in this.dbContext.Sms
                               join relationship in this.dbContext.Relationships on new { SmsId = sms.Id, Type = RelationshipType.PolicyMessage, FromEntityType = EntityType.Policy } equals new { SmsId = relationship.ToEntityId, Type = relationship.Type, FromEntityType = relationship.FromEntityType }
                               select new
                               {
                                   Sms = sms,
                                   RelationShip = relationship,
                               };

                query = query.GroupJoin(smsQuery, p => p.Policy.Id, c => c.RelationShip.FromEntityId, (p, sms) => new PolicyReadModelWithRelatedEntities
                {
                    Policy = p.Policy,
                    Customer = p.Customer,
                    Tenant = p.Tenant,
                    Organisation = p.Organisation,
                    TenantDetails = p.TenantDetails,
                    Product = p.Product,
                    ProductDetails = p.ProductDetails,
                    Owner = p.Owner,
                    QuoteDocuments = p.QuoteDocuments,
                    ClaimDocuments = p.ClaimDocuments,
                    Emails = p.Emails,
                    Quotes = p.Quotes,
                    Claims = p.Claims,
                    PolicyTransactions = p.PolicyTransactions,
                    Sms = sms.Select(s => s.Sms),
                    FromRelationships = p.FromRelationships,
                    ToRelationships = p.ToRelationships,
                    TextAdditionalPropertiesValues = p.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = p.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedProperties.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Policy.Quotes))))
            {
                query = query.GroupJoin(this.dbContext.QuoteReadModels, p => p.Policy.Id, c => c.PolicyId, (p, quotes) => new PolicyReadModelWithRelatedEntities
                {
                    Policy = p.Policy,
                    Customer = p.Customer,
                    Tenant = p.Tenant,
                    Organisation = p.Organisation,
                    TenantDetails = p.TenantDetails,
                    Product = p.Product,
                    ProductDetails = p.ProductDetails,
                    Owner = p.Owner,
                    QuoteDocuments = p.QuoteDocuments,
                    ClaimDocuments = p.ClaimDocuments,
                    Emails = p.Emails,
                    Quotes = quotes.OrderByDescending(q => q.CreatedTicksSinceEpoch),
                    Claims = p.Claims,
                    PolicyTransactions = p.PolicyTransactions.OrderByDescending(pt => pt.CreatedTicksSinceEpoch),
                    Sms = p.Sms,
                    FromRelationships = p.FromRelationships,
                    ToRelationships = p.ToRelationships,
                    TextAdditionalPropertiesValues = p.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = p.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedProperties.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Policy.Claims))))
            {
                query = query.GroupJoin(this.dbContext.ClaimReadModels, p => p.Policy.Id, c => c.PolicyId, (p, claims) => new PolicyReadModelWithRelatedEntities
                {
                    Policy = p.Policy,
                    Customer = p.Customer,
                    Tenant = p.Tenant,
                    Organisation = p.Organisation,
                    TenantDetails = p.TenantDetails,
                    Product = p.Product,
                    ProductDetails = p.ProductDetails,
                    Owner = p.Owner,
                    QuoteDocuments = p.QuoteDocuments,
                    ClaimDocuments = p.ClaimDocuments,
                    Emails = p.Emails,
                    Quotes = p.Quotes,
                    Claims = claims.Where(c => c.Status != ClaimState.Nascent),
                    PolicyTransactions = p.PolicyTransactions,
                    Sms = p.Sms,
                    FromRelationships = p.FromRelationships,
                    ToRelationships = p.ToRelationships,
                    TextAdditionalPropertiesValues = p.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = p.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedProperties.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Policy.Relationships))))
            {
                query = query.GroupJoin(this.dbContext.Relationships, p => p.Policy.Id, r => r.FromEntityId, (p, relationships) => new PolicyReadModelWithRelatedEntities
                {
                    Policy = p.Policy,
                    Customer = p.Customer,
                    Tenant = p.Tenant,
                    Organisation = p.Organisation,
                    TenantDetails = p.TenantDetails,
                    Product = p.Product,
                    ProductDetails = p.ProductDetails,
                    Owner = p.Owner,
                    QuoteDocuments = p.QuoteDocuments,
                    ClaimDocuments = p.ClaimDocuments,
                    Emails = p.Emails,
                    Quotes = p.Quotes,
                    Claims = p.Claims,
                    PolicyTransactions = p.PolicyTransactions,
                    Sms = p.Sms,
                    FromRelationships = relationships,
                    ToRelationships = p.ToRelationships,
                    TextAdditionalPropertiesValues = p.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = p.StructuredDataAdditionalPropertyValues,
                });

                query = query.GroupJoin(this.dbContext.Relationships, p => p.Policy.Id, r => r.ToEntityId, (p, relationships) => new PolicyReadModelWithRelatedEntities
                {
                    Policy = p.Policy,
                    Customer = p.Customer,
                    Tenant = p.Tenant,
                    Organisation = p.Organisation,
                    TenantDetails = p.TenantDetails,
                    Product = p.Product,
                    ProductDetails = p.ProductDetails,
                    Owner = p.Owner,
                    QuoteDocuments = p.QuoteDocuments,
                    ClaimDocuments = p.ClaimDocuments,
                    Emails = p.Emails,
                    Quotes = p.Quotes,
                    Claims = p.Claims,
                    PolicyTransactions = p.PolicyTransactions,
                    Sms = p.Sms,
                    FromRelationships = p.FromRelationships,
                    ToRelationships = relationships,
                    TextAdditionalPropertiesValues = p.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = p.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedProperties.Any(p => p.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Policy.AdditionalProperties))))
            {
                query = query.GroupJoin(
                    this.dbContext.TextAdditionalPropertValues.IncludeAllProperties(),
                    p => p.Policy.Id,
                    apv => apv.EntityId,
                    (p, additionalProperties) => new PolicyReadModelWithRelatedEntities
                    {
                        Policy = p.Policy,
                        Customer = p.Customer,
                        Tenant = p.Tenant,
                        Organisation = p.Organisation,
                        TenantDetails = p.TenantDetails,
                        Product = p.Product,
                        ProductDetails = p.ProductDetails,
                        Owner = p.Owner,
                        QuoteDocuments = p.QuoteDocuments,
                        ClaimDocuments = p.ClaimDocuments,
                        Emails = p.Emails,
                        Quotes = p.Quotes,
                        Claims = p.Claims,
                        PolicyTransactions = p.PolicyTransactions,
                        Sms = p.Sms,
                        FromRelationships = p.FromRelationships,
                        ToRelationships = p.ToRelationships,
                        TextAdditionalPropertiesValues = (IEnumerable<TextAdditionalPropertyValueReadModel>)additionalProperties
                            .Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                        StructuredDataAdditionalPropertyValues = p.StructuredDataAdditionalPropertyValues,
                    })
                    .GroupJoin(
                    this.dbContext.StructuredDataAdditionalPropertyValues.IncludeAllProperties(),
                    p => p.Policy.Id,
                    apv => apv.EntityId,
                    (p, additionalProperties) => new PolicyReadModelWithRelatedEntities
                    {
                        Policy = p.Policy,
                        Customer = p.Customer,
                        Tenant = p.Tenant,
                        Organisation = p.Organisation,
                        TenantDetails = p.TenantDetails,
                        Product = p.Product,
                        ProductDetails = p.ProductDetails,
                        Owner = p.Owner,
                        QuoteDocuments = p.QuoteDocuments,
                        ClaimDocuments = p.ClaimDocuments,
                        Emails = p.Emails,
                        Quotes = p.Quotes,
                        Claims = p.Claims,
                        PolicyTransactions = p.PolicyTransactions,
                        Sms = p.Sms,
                        FromRelationships = p.FromRelationships,
                        ToRelationships = p.ToRelationships,
                        TextAdditionalPropertiesValues = p.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = (IEnumerable<StructuredDataAdditionalPropertyValueReadModel>)additionalProperties
                            .Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                    });
            }

            return query;
        }

        private IQueryable<PolicyQuoteProductCustomerOrganisationReadModel> JoinQuotePolicyProductOrganisation(
            IQueryable<PolicyReadModel> query)
        {
            var policyQuoteQuery = query
                .GroupJoin(
                    this.dbContext.QuoteReadModels,
                    q => q.QuoteId,
                    p => p.Id,
                    (p, q) => new
                    {
                        Policy = p,
                        Quote = q.FirstOrDefault(),
                    });

            var policyQuoteCustomer = policyQuoteQuery.GroupJoin(
                     this.dbContext.CustomerReadModels,
                     model => model.Policy.CustomerId,
                     customer => customer.Id,
                     (model, customer) => new
                     {
                         Policy = model.Policy,
                         Quote = model.Quote,
                         Customer = customer.FirstOrDefault(),
                     });

            var policyQuoteCustomerProduct = policyQuoteCustomer.Join(
                    this.dbContext.Products.Include(p => p.DetailsCollection),
                    pq => pq.Policy.ProductId,
                    prod => prod.Id,
                    (model, prod) => new
                    {
                        Policy = model.Policy,
                        Quote = model.Quote,
                        Customer = model.Customer,
                        Product = prod,
                    });

            var policyQuoteProductCustomerOrganisationQuery = policyQuoteCustomerProduct.Join(
                    this.dbContext.OrganisationReadModel,
                    policyAndQuoteAndProduct => new { organisationId = policyAndQuoteAndProduct.Policy.OrganisationId },
                    organisation => new { organisationId = organisation.Id },
                    (model, organisation) => new PolicyQuoteProductCustomerOrganisationReadModel
                    {
                        Policy = model.Policy,
                        Quote = model.Quote,
                        Product = model.Product,
                        Customer = model.Customer,
                        Organisation = organisation,
                    });

            return policyQuoteProductCustomerOrganisationQuery;
        }

        /// <summary>
        /// Gets query for policies for export from a given tenant and environment that match given filters.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>A query for policies.</returns>
        private IEnumerable<IPolicyReadModelSummary> GetPoliciesForExport(Guid tenantId, PolicyReadModelFilters filters)
        {
            var parameters = new DynamicParameters();
            var sqlQuery = new StringBuilder();
            var queryFilter = this.QueryFilterForPolicyReadModel(filters);

            sqlQuery.AppendLine(PolicyRepositoryResourceScript.GetPolicies);
            sqlQuery.AppendLine(queryFilter);

            parameters.Add("@TenantId", tenantId);
            parameters.Add("@Environment", filters.Environment);

            if (filters.PageSize > 0)
            {
                sqlQuery.AppendLine("OFFSET @Offset ROWS");
                sqlQuery.AppendLine("FETCH NEXT @Next ROWS ONLY");
                parameters.Add("@Offset", filters.PageSize * (filters.Page - 1));
                parameters.Add("@Next", filters.PageSize);
            }

            using (var connection = new SqlConnection(this.connection.UBind))
            {
                var policyReadModels = connection.Query<PolicyReadModelSummary>(
                    sqlQuery.ToString(),
                    parameters,
                    null,
                    true,
                    180,
                    System.Data.CommandType.Text).ToList();
                return policyReadModels;
            }
        }

        private string QueryFilterForPolicyReadModel(PolicyReadModelFilters filters)
        {
            var query = new StringBuilder();

            // make sure it actually has a policy. There was some code which created policy read model records
            // at the time of quote creation which created thousands of unnecessary records.
            // If we don't filter them out, they'll have null dates, and then our projection will cause an
            // exception. We know it really has a policy if it has an inception time.
            query.AppendLine("AND PRM.InceptionTicksSinceEpoch > 0");

            if (filters.OwnerUserId.HasValue && filters.OwnerUserId != default)
            {
                query.AppendLine($"AND PRM.OwnerUserId = '{filters.OwnerUserId}'");
            }

            if (filters.CustomerId.HasValue && filters.CustomerId != default)
            {
                query.AppendLine($"AND PRM.CustomerId = '{filters.CustomerId}'");
            }

            bool hasOrganisationFilter = filters.OrganisationIds != null && filters.OrganisationIds.Any();
            if (hasOrganisationFilter)
            {
                string guidList = string.Join(", ", filters.OrganisationIds.Select(g => $"'{g}'"));
                query.AppendLine($"AND PRM.OrganisationId IN ({guidList})");
            }

            if (!filters.IncludeTestData)
            {
                query.AppendLine("AND PRM.IsTestData = 0");
            }

            if (filters.ProductId != null)
            {
                query.AppendLine($"AND PRM.ProductId = '{filters.ProductId}'");
            }

            if (!string.IsNullOrEmpty(filters.PolicyNumber))
            {
                query.AppendLine($"AND PRM.PolicyNumber = '{filters.PolicyNumber}'");
            }

            if (filters.PolicyId != null)
            {
                query.AppendLine($"AND PRM.Id = '{filters.PolicyId}'");
            }

            if (filters.Statuses.Any())
            {
                var queryStatus = string.Empty;
                var counter = 0;
                var currentTicksSinceEpoch = this.clock.GetCurrentInstant().ToUnixTimeTicks();
                foreach (var status in filters.Statuses.Where(x => x != null).Select(s => s.ToEnumOrThrow<PolicyStatus>()))
                {
                    if (counter == 0)
                    {
                        queryStatus = this.GetQueryStringForPolicyStatusMatching(status, currentTicksSinceEpoch);
                    }
                    else
                    {
                        queryStatus = queryStatus + " OR " + this.GetQueryStringForPolicyStatusMatching(status, currentTicksSinceEpoch);
                    }

                    counter++;
                }

                query.AppendLine($"AND {queryStatus}");
            }

            query.AppendLine("ORDER BY PRM.CreatedTicksSinceEpoch DESC");

            return query.ToString();
        }

        private string GetQueryStringForPolicyStatusMatching(PolicyStatus status, long currentTicksSinceEpoch)
        {
            if (status == PolicyStatus.Cancelled)
            {
                return $"(PRM.CancellationEffectiveTicksSinceEpoch IS NOT NULL) AND " +
                    $"((PRM.CancellationEffectiveTicksSinceEpoch < {currentTicksSinceEpoch}) OR " +
                        $"(PRM.CancellationEffectiveTicksSinceEpoch = PRM.InceptionTicksSinceEpoch))";
            }

            if (status == PolicyStatus.Issued)
            {
                return $"((PRM.CancellationEffectiveTicksSinceEpoch IS NULL) OR " +
                        $"((PRM.CancellationEffectiveTicksSinceEpoch > {currentTicksSinceEpoch}) AND " +
                            $"(PRM.CancellationEffectiveTicksSinceEpoch != PRM.InceptionTicksSinceEpoch))) " +
                    $" AND (PRM.InceptionTicksSinceEpoch > {currentTicksSinceEpoch})";
            }

            if (status == PolicyStatus.Active)
            {
                return $"((PRM.CancellationEffectiveTicksSinceEpoch IS NULL) OR " +
                        $"((PRM.CancellationEffectiveTicksSinceEpoch > {currentTicksSinceEpoch}) AND " +
                            $"(PRM.CancellationEffectiveTicksSinceEpoch != PRM.InceptionTicksSinceEpoch))) " +
                    $" AND (PRM.InceptionTicksSinceEpoch < {currentTicksSinceEpoch}) " +
                    $" AND (PRM.ExpiryTicksSinceEpoch > {currentTicksSinceEpoch})";
            }

            if (status == PolicyStatus.Expired)
            {
                return $"((PRM.CancellationEffectiveTicksSinceEpoch IS NULL) OR " +
                        $"((PRM.CancellationEffectiveTicksSinceEpoch > {currentTicksSinceEpoch}) AND " +
                            $"(PRM.CancellationEffectiveTicksSinceEpoch != PRM.InceptionTicksSinceEpoch))) " +
                    $" AND (PRM.ExpiryTicksSinceEpoch < {currentTicksSinceEpoch})";
            }

            if (status == PolicyStatus.IssuedOrActive)
            {
                return $"((PRM.CancellationEffectiveTicksSinceEpoch IS NULL) OR " +
                        $"((PRM.CancellationEffectiveTicksSinceEpoch > {currentTicksSinceEpoch}) AND " +
                            $"(PRM.CancellationEffectiveTicksSinceEpoch != PRM.InceptionTicksSinceEpoch)))" +
                    $" AND (PRM.ExpiryTicksSinceEpoch > {currentTicksSinceEpoch})";
            }

            if (status == PolicyStatus.ExpiredOrCancelled)
            {
                return $"((PRM.CancellationEffectiveTicksSinceEpoch IS NOT NULL) AND " +
                        $"((PRM.CancellationEffectiveTicksSinceEpoch < {currentTicksSinceEpoch}) OR " +
                            $"(PRM.CancellationEffectiveTicksSinceEpoch = PRM.InceptionTicksSinceEpoch)))" +
                    $" OR (PRM.ExpiryTicksSinceEpoch < {currentTicksSinceEpoch})";
            }

            if (status == PolicyStatus.Any)
            {
                return string.Empty;
            }

            throw new InvalidOperationException($"Cannot filter on policy status {status.ToString()}");
        }

        private Expression<Func<PolicyReadModel, bool>> GetExpressionForStatusMatching(PolicyStatus status, long currentTicksSinceEpoch)
        {
            if (status == PolicyStatus.Cancelled)
            {
                return policy =>
                    (policy.CancellationEffectiveTicksSinceEpoch != null) &&
                    ((policy.CancellationEffectiveTicksSinceEpoch < currentTicksSinceEpoch) ||
                        (policy.CancellationEffectiveTicksSinceEpoch == policy.InceptionTicksSinceEpoch));
            }

            if (status == PolicyStatus.Issued)
            {
                return policy =>
                    ((policy.CancellationEffectiveTicksSinceEpoch == null) ||
                        ((policy.CancellationEffectiveTicksSinceEpoch > currentTicksSinceEpoch) &&
                            (policy.CancellationEffectiveTicksSinceEpoch != policy.InceptionTicksSinceEpoch)))
                    && (policy.InceptionTicksSinceEpoch > currentTicksSinceEpoch);
            }

            if (status == PolicyStatus.Active)
            {
                return policy =>
                    ((policy.CancellationEffectiveTicksSinceEpoch == null) ||
                        ((policy.CancellationEffectiveTicksSinceEpoch > currentTicksSinceEpoch) &&
                            (policy.CancellationEffectiveTicksSinceEpoch != policy.InceptionTicksSinceEpoch)))
                    && (policy.InceptionTicksSinceEpoch < currentTicksSinceEpoch)
                    && (policy.ExpiryTicksSinceEpoch > currentTicksSinceEpoch);
            }

            if (status == PolicyStatus.Expired)
            {
                return policy =>
                    ((policy.CancellationEffectiveTicksSinceEpoch == null) ||
                        ((policy.CancellationEffectiveTicksSinceEpoch > currentTicksSinceEpoch) &&
                            (policy.CancellationEffectiveTicksSinceEpoch != policy.InceptionTicksSinceEpoch)))
                    && (policy.ExpiryTicksSinceEpoch < currentTicksSinceEpoch);
            }

            if (status == PolicyStatus.IssuedOrActive)
            {
                return policy =>
                    ((policy.CancellationEffectiveTicksSinceEpoch == null) ||
                        ((policy.CancellationEffectiveTicksSinceEpoch > currentTicksSinceEpoch) &&
                            (policy.CancellationEffectiveTicksSinceEpoch != policy.InceptionTicksSinceEpoch)))
                    && (policy.ExpiryTicksSinceEpoch > currentTicksSinceEpoch);
            }

            if (status == PolicyStatus.ExpiredOrCancelled)
            {
                return policy =>
                    ((policy.CancellationEffectiveTicksSinceEpoch != null) &&
                        ((policy.CancellationEffectiveTicksSinceEpoch < currentTicksSinceEpoch) ||
                            (policy.CancellationEffectiveTicksSinceEpoch == policy.InceptionTicksSinceEpoch)))
                    || (policy.ExpiryTicksSinceEpoch < currentTicksSinceEpoch);
            }

            if (status == PolicyStatus.Any)
            {
                return policy => true;
            }

            throw new InvalidOperationException("Cannot filter on policy status " + status.ToString());
        }

        private IEnumerable<Guid> GetPolicyIds(IQueryable<PolicyReadModel> policies, PolicyReadModelFilters filters)
        {
            return policies
                .Join(
                    this.dbContext.Products.Include(p => p.DetailsCollection),
                    policy => new { productId = policy.ProductId, tenantId = policy.TenantId },
                    product => new { productId = product.Id, tenantId = product.TenantId },
                    (policy, product) => new PolicyAndProductAndProductFeature { Policy = policy, Product = product })
                .OrderByDescending(pp => pp.Policy.CreatedTicksSinceEpoch)
                .Select(p => p.Policy.Id)
                .Paginate(filters)
                .ToList();
        }

        private IEnumerable<IPolicyReadModelSummary> GetSummaries(IQueryable<PolicyReadModel> policies, EntityListFilters filters)
        {
            if (filters.IncludeProductFeatureSetting)
            {
                return policies.Join(
                    this.dbContext.ProductFeatureSetting,
                    policy => new { productId = policy.ProductId, tenantId = policy.TenantId },
                    productFeature => new { productId = productFeature.ProductId, tenantId = productFeature.TenantId },
                    (policy, productFeature) => new { Policy = policy, ProductFeature = productFeature })
                    .Join(
                        this.dbContext.Products,
                        model => new { tenantId = model.Policy.TenantId, productId = model.Policy.ProductId },
                        product => new { tenantId = product.TenantId, productId = product.Id },
                        (model, product) => new PolicyAndProductAndProductFeature { Policy = model.Policy, ProductFeature = model.ProductFeature, Product = product })
                    .Select(this.SummarySelector)
                    .ToList();
            }
            else
            {
                return policies
                    .Join(
                        this.dbContext.Products.Include(p => p.DetailsCollection),
                        policy => new { productId = policy.ProductId, tenantId = policy.TenantId },
                        product => new { productId = product.Id, tenantId = product.TenantId },
                        (policy, product) => new PolicyAndProductAndProductFeature { Policy = policy, Product = product, ProductFeature = null })
                    .Select(this.SummarySelector)
                    .ToList();
            }
        }

        private class PolicyAndProductAndProductFeature
        {
            public PolicyReadModel Policy { get; set; }

            public Product Product { get; set; }

            public ProductFeatureSetting ProductFeature { get; set; }
        }

        private class PolicyProductOrganisation
        {
            public PolicyReadModel Policy { get; set; }

            public Product Product { get; set; }

            public OrganisationReadModel Organisation { get; set; }
        }

        private class Policy
        {
            public PolicyReadModel PolicyReadModel { get; set; }
        }
    }
}
