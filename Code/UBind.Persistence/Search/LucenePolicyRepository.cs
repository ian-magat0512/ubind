// <copyright file="LucenePolicyRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Search
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Humanizer;
    using Lucene.Net.Analysis;
    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.Queries;
    using Lucene.Net.QueryParsers.Classic;
    using Lucene.Net.Search;
    using Lucene.Net.Util;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Search;

    public class LucenePolicyRepository : BaseLuceneRepository<IPolicySearchIndexWriteModel, IPolicySearchResultItemReadModel>,
        ILuceneRepository<IPolicySearchIndexWriteModel, IPolicySearchResultItemReadModel, PolicyReadModelFilters>
    {
        private readonly ILogger<LucenePolicyRepository> logger;
        private readonly string lastUpdatedTimestampField = PolicyLuceneFieldNames.FieldLastModifiedTicksSinceEpoch;
        private readonly string lastUpdatedByUserTimestampField = PolicyLuceneFieldNames.FieldLastModifiedByUserTicksSinceEpoch;

        public LucenePolicyRepository(
            ILuceneDirectoryConfiguration luceneConfig,
            ILuceneDocumentBuilder<IPolicySearchIndexWriteModel> luceneDocumentBuilder,
            IClock clock,
            ILuceneIndexCache luceneIndexCache,
            ICachingResolver cachingResolver,
            ILogger<LucenePolicyRepository> logger)
            : base(luceneConfig, luceneDocumentBuilder, luceneIndexCache, LuceneIndexType.Policy, cachingResolver, clock)
        {
            this.logger = logger;
            this.Analyzer = new CustomLuceneAnalyzer();
        }

        public override Analyzer Analyzer
        {
            get { return base.Analyzer; }
            set { base.Analyzer = value; }
        }

        protected override Dictionary<string, string> DateFilteringPropertyNameToLuceneFieldMap =>
           new Dictionary<string, string>
           {
                { nameof(PolicySearchResultItemReadModel.IssuedTicksSinceEpoch), PolicyLuceneFieldNames.FieldIssuedTicksSinceEpoch },
                { nameof(PolicySearchResultItemReadModel.InceptionTicksSinceEpoch), PolicyLuceneFieldNames.FieldInceptionTicksSinceEpoch },
                { nameof(PolicySearchResultItemReadModel.ExpiryTicksSinceEpoch), PolicyLuceneFieldNames.FieldExpiryTicksSinceEpoch },
                { nameof(PolicySearchResultItemReadModel.LatestRenewalEffectiveTicksSinceEpoch), PolicyLuceneFieldNames.FieldLatestRenewalEffectiveTicksSinceEpoch },
                { nameof(PolicySearchResultItemReadModel.CancellationEffectiveTicksSinceEpoch), PolicyLuceneFieldNames.FieldCancellationEffectiveTicksSinceEpoch },
                { nameof(PolicySearchResultItemReadModel.LastModifiedTicksSinceEpoch), PolicyLuceneFieldNames.FieldLastModifiedTicksSinceEpoch },
           };

        protected override Dictionary<string, (string, SortFieldType)> SortingPropertyNameToLuceneFieldMap =>
            new Dictionary<string, (string, SortFieldType)>
            {
                { nameof(PolicySearchResultItemReadModel.CreatedTicksSinceEpoch), (PolicyLuceneFieldNames.FieldCreatedTicksSinceEpoch, SortFieldType.INT64) },
                { nameof(PolicySearchResultItemReadModel.IssuedTicksSinceEpoch), (PolicyLuceneFieldNames.FieldIssuedTicksSinceEpoch, SortFieldType.INT64) },
                { nameof(PolicySearchResultItemReadModel.InceptionTicksSinceEpoch), (PolicyLuceneFieldNames.FieldInceptionTicksSinceEpoch, SortFieldType.INT64) },
                { nameof(PolicySearchResultItemReadModel.ExpiryTicksSinceEpoch), (PolicyLuceneFieldNames.FieldExpiryTicksSinceEpoch, SortFieldType.INT64) },
                { nameof(PolicySearchResultItemReadModel.LatestRenewalEffectiveTicksSinceEpoch), (PolicyLuceneFieldNames.FieldLatestRenewalEffectiveTicksSinceEpoch, SortFieldType.INT64) },
                { nameof(PolicySearchResultItemReadModel.CancellationEffectiveTicksSinceEpoch), (PolicyLuceneFieldNames.FieldCancellationEffectiveTicksSinceEpoch, SortFieldType.INT64) },
                { nameof(PolicySearchResultItemReadModel.LastModifiedTicksSinceEpoch), (PolicyLuceneFieldNames.FieldLastModifiedTicksSinceEpoch, SortFieldType.INT64) },
                { nameof(PolicySearchResultItemReadModel.CustomerFullName), (PolicyLuceneFieldNames.FieldCustomerFullName, SortFieldType.STRING) },
                { nameof(PolicySearchResultItemReadModel.PolicyNumber), (PolicyLuceneFieldNames.FieldPolicyNumber, SortFieldType.STRING) },
            };

        private MultiFieldQueryParser PolicyQueryParser
        {
            get
            {
                var searchFields = new string[]
                {
                    PolicyTransactionLuceneFieldNames.FieldPolicyDataSerializedCalculationResult,
                    PolicyTransactionLuceneFieldNames.FieldPolicyDataFormData,
                    PolicyLuceneFieldNames.FieldSerializedCalculationResult,
                    PolicyLuceneFieldNames.FieldCustomerEmail,
                    PolicyLuceneFieldNames.FieldCustomerPreferredName,
                    PolicyLuceneFieldNames.FieldCustomerFullName,
                    PolicyLuceneFieldNames.FieldPolicyNumber,
                };

                MultiFieldQueryParser queryParser = new MultiFieldQueryParser(
                    LuceneVersion.LUCENE_48,
                    searchFields,
                    this.Analyzer)
                {
                    DefaultOperator = QueryParser.AND_OPERATOR,
                };

                queryParser.AllowLeadingWildcard = false;

                return queryParser;
            }
        }

        /// <inheritdoc/>
        public long? GetIndexLastUpdatedTicksSinceEpoch(Tenant tenant, DeploymentEnvironment environment)
        {
            return this.GetEntityIndexLastUpdatedTimestamp(tenant, environment, this.lastUpdatedTimestampField, this.lastUpdatedByUserTimestampField);
        }

        /// <inheritdoc/>
        public int GetEntityIndexCountBetweenDates(Tenant tenant, DeploymentEnvironment environment, Instant fromDateTime, Instant toDateTime)
        {
            var filters = new EntityListFilters
            {
                DateFilteringPropertyName = nameof(PolicySearchResultItemReadModel.LastModifiedTicksSinceEpoch),
                DateIsAfterTicks = fromDateTime.ToUnixTimeTicks(),
                DateIsBeforeTicks = toDateTime.ToUnixTimeTicks(),
            };

            return this.GetLuceneIndexCountBetweenDates(tenant, environment, filters, this.lastUpdatedTimestampField);
        }

        /// <inheritdoc/>
        public IEnumerable<IPolicySearchResultItemReadModel> Search(
            Tenant tenant, DeploymentEnvironment environment, PolicyReadModelFilters filters)
        {
            if (!this.SortingPropertyNameToLuceneFieldMap.TryGetValue(filters.SortBy, out (string, SortFieldType) sortFieldResult))
            {
                throw new ErrorException(Errors.SearchIndex.InvalidSortingPropertyName(filters.SortBy));
            }

            var (sortByLuceneField, sortFieldType) = sortFieldResult;
            Sort sort = this.GetEntitySorting(sortByLuceneField, sortFieldType, filters.SortOrder);
            TopFieldDocs searchResult;

            var searchResults = Enumerable.Empty<PolicySearchResultItemReadModel>();
            DirectoryInfo indexDirectory = this.GetLatestLiveIndexDirectory(tenant, environment);
            if (indexDirectory == null || !indexDirectory.Exists)
            {
                // no policy have been indexed, since we don't even have an index directory yet.
                this.logger.LogInformation($"Lucene index directory was not found. It's likely that no policy have been indexed "
                    + $" yet in this environment. Tenant={tenant.Details.Alias}, Environment={environment}.");
                return searchResults;
            }

            var searcher = this.CreateIndexSearcher(indexDirectory);

            if (filters.SearchTerms.Any())
            {
                BooleanQuery termsBooleanQuery = this.GetEntitySearchTermsQuery(filters, this.PolicyQueryParser);

                if (filters.Statuses.Any())
                {
                    foreach (var status in filters.Statuses.Where(x => x != null && Enum.IsDefined(typeof(PolicyStatus), x)).Select(s => s.ToEnumOrThrow<PolicyStatus>()))
                    {
                        // This needs to be reset each loop as BooleanFilter is a ref type.
                        BooleanFilter entityFieldFilter = this.GenerateEntityFieldFilter(filters);
                        searchResults = this.GetPolicySearchByTerm(searcher, sort, entityFieldFilter, DefaultHitsCap, searchResults, status, termsBooleanQuery);
                    }
                }
                else
                {
                    BooleanFilter entityFieldFilter = this.GenerateEntityFieldFilter(filters);
                    searchResult = searcher.Search(termsBooleanQuery, entityFieldFilter, DefaultHitsCap, sort);
                    searchResults = Enumerable.Concat(searchResults, this.GetSearchResultReadModels(searcher, searchResult));
                }

                if (filters.IsRideProtectOrganisation)
                {
                    BooleanFilter entityFieldFilterForRideProtect =
                        this.GenerateEntityFieldFilterForRideProtectProductFromOtherOrganisation(filters);
                    var searchResultForRideProtect = searcher.Search(termsBooleanQuery, entityFieldFilterForRideProtect, DefaultHitsCap, sort);
                    searchResults = Enumerable.Concat(searchResults, this.GetSearchResultReadModels(searcher, searchResultForRideProtect));
                }
            }
            else
            {
                foreach (var status in filters.Statuses.Where(x => x != null && Enum.IsDefined(typeof(PolicyStatus), x)).Select(s => s.ToEnumOrThrow<PolicyStatus>()))
                {
                    // This needs to be reset each loop as BooleanFilter is a ref type.
                    BooleanFilter entityFieldFilter = this.GenerateEntityFieldFilter(filters);
                    searchResults = this.GetPolicySearch(searcher, sort, entityFieldFilter, DefaultHitsCap, searchResults, status);

                    if (filters.IsRideProtectOrganisation)
                    {
                        BooleanFilter entityFieldFilterForRideProtect =
                            this.GenerateEntityFieldFilterForRideProtectProductFromOtherOrganisation(filters);
                        searchResults = this.GetPolicySearch(searcher, sort, entityFieldFilterForRideProtect, DefaultHitsCap, searchResults, status);
                    }
                }

                searcher.IndexReader.Dispose();
            }

            if (!string.IsNullOrEmpty(filters.SortBy))
            {
                searchResults = searchResults.AsQueryable().Order(filters.SortBy, filters.SortOrder);
            }

            return this.GetEntityListInRange(searchResults.ToList(), filters);
        }

        /// <inheritdoc/>
        public void AddItemsToIndex(
            Tenant tenant,
            DeploymentEnvironment environment,
            IEnumerable<IPolicySearchIndexWriteModel> policies)
        {
            Dictionary<Guid, Document> documentsDictionary = this.GenerateEntityLuceneDocuments(policies);
            DirectoryInfo indexDirectory = this.GetLatestLiveIndexDirectory(tenant, environment);
            if (indexDirectory == null)
            {
                indexDirectory = this.CreateNewLuceneDirectory(tenant, environment);
            }

            this.UpsertDocumentsToLuceneIndex(indexDirectory, documentsDictionary);
            this.SetIndexLastUpdatedIndexCacheKey(tenant, environment, this.lastUpdatedTimestampField, this.lastUpdatedByUserTimestampField);
        }

        /// <inheritdoc/>
        public void AddItemsToRegenerationIndex(
            Tenant tenant,
            DeploymentEnvironment environment,
            IEnumerable<IPolicySearchIndexWriteModel> policies)
        {
            var documentsDictionary = this.GenerateEntityLuceneDocuments(policies);
            var indexDirectory = this.GetOrCreateRegenerationIndexDirectory(tenant, environment);
            this.UpsertDocumentsToLuceneIndex(indexDirectory, documentsDictionary);
            this.RemoveIndexLastUpdatedTimestampCache(tenant, environment, this.lastUpdatedTimestampField);
        }

        /// <inheritdoc/>
        public void DeleteItemsFromIndex(Tenant tenant, DeploymentEnvironment environment, IEnumerable<Guid> policyIds)
        {
            this.DeleteEntityItemsFromIndex(tenant, environment, policyIds);
        }

        /// <inheritdoc/>
        public void MakeRegenerationIndexTheLiveIndex(Tenant tenant, DeploymentEnvironment environment)
        {
            this.MakeEntityRegenerationIndexTheLiveIndex(tenant, environment);
        }

        /// <inheritdoc/>
        public void MakeSureRegenerationFolderIsEmptyBeforeRegeneration(Tenant tenant, DeploymentEnvironment environment)
        {
            this.MakeSureEntityRegenerationFolderIsEmptyBeforeRegeneration(tenant, environment);
        }

        private BooleanFilter GenerateEntityFieldFilter(PolicyReadModelFilters filters)
        {
            List<FilterClause> filterClause = new List<FilterClause>()
            {
                this.GetProductFilterClause(filters, PolicyLuceneFieldNames.FieldProductId),
                this.GetDateRangeFilterClause(filters),
                this.GetEntityGuidFieldFilterClause(filters.CustomerId, PolicyLuceneFieldNames.FieldCustomerId),
                this.GetOrganisationFilterClause(filters, PolicyLuceneFieldNames.FieldOrganisationId),
                this.GetEntityGuidFieldFilterClause(filters.OwnerUserId, PolicyLuceneFieldNames.FieldOwnerUserId),
            };

            if (!filters.IncludeTestData)
            {
                filterClause.Add(this.GetBooleanFieldFilterClause(filters.IncludeTestData, QuoteLuceneFieldsNames.IsTestData));
            }

            return this.GenerateBooleanEntityReadModelFilter(filterClause);
        }

        /// <summary>
        /// This is a temporary hardcode method specific for ride-protect sub-organisation,
        /// to be removed after the implementation of UB-8372.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns>A bolean filter.</returns>
        private BooleanFilter GenerateEntityFieldFilterForRideProtectProductFromOtherOrganisation(PolicyReadModelFilters filters)
        {
            var productsFilter = new TermsFilter(new Term(PolicyLuceneFieldNames.FieldProductId, filters.RideProtectProductId.ToString()));
            var productFilterClause = new FilterClause(productsFilter, Occur.MUST);
            var organisationFilterClause = this.GetOrganisationFilterClause(filters, PolicyLuceneFieldNames.FieldOrganisationId, Occur.MUST_NOT);
            List<FilterClause> filterClause = new List<FilterClause>()
            {
                productFilterClause,
                organisationFilterClause,
                this.GetDateRangeFilterClause(filters),
                this.GetEntityGuidFieldFilterClause(filters.CustomerId, PolicyLuceneFieldNames.FieldCustomerId),
                this.GetEntityGuidFieldFilterClause(filters.OwnerUserId, PolicyLuceneFieldNames.FieldOwnerUserId),
            };

            if (!filters.SearchTerms.Any() && !filters.IncludeTestData)
            {
                filterClause.Add(this.GetBooleanFieldFilterClause(filters.IncludeTestData, QuoteLuceneFieldsNames.IsTestData));
            }

            return this.GenerateBooleanEntityReadModelFilter(filterClause);
        }

        private IEnumerable<PolicySearchResultItemReadModel> GetSearchResultReadModels(
            IndexSearcher searcher, TopFieldDocs searchResults)
        {
            var results = new List<PolicySearchResultItemReadModel>();

            foreach (var documentNumber in searchResults.ScoreDocs.Select(s => s.Doc))
            {
                var documentToParse = searcher.Doc(documentNumber);
                var policyId = Guid.Parse(documentToParse.Get(PolicyLuceneFieldNames.FieldPolicyId));
                var createdTimestamp = long.Parse(documentToParse.Get(PolicyLuceneFieldNames.FieldCreatedTicksSinceEpoch));
                var lastUpdatedTimestamp = long.Parse(documentToParse.Get(PolicyLuceneFieldNames.FieldLastModifiedByUserTicksSinceEpoch)
                    ?? documentToParse.Get(PolicyLuceneFieldNames.FieldLastModifiedTicksSinceEpoch));
                results.Add(new PolicySearchResultItemReadModel(documentToParse, policyId, createdTimestamp, lastUpdatedTimestamp));
            }

            return results;
        }

        private IEnumerable<PolicySearchResultItemReadModel> GetPolicySearch(
            IndexSearcher searcher,
            Sort sort,
            BooleanFilter booleanReadModelFilter,
            int hitsCap,
            IEnumerable<PolicySearchResultItemReadModel> models,
            PolicyStatus status)
        {
            var filterWithStatus = this.GetStatusFilter(booleanReadModelFilter, status.Humanize(), PolicyLuceneFieldNames.FieldPolicyState);
            var searchResult = this.SearchAllEntityDocuments(searcher, sort, hitsCap, filterWithStatus);
            return Enumerable.Concat(models, this.GetSearchResultReadModels(searcher, searchResult));
        }

        private IEnumerable<PolicySearchResultItemReadModel> GetPolicySearchByTerm(
            IndexSearcher searcher,
            Sort sort,
            BooleanFilter booleanReadModelFilter,
            int hitsCap,
            IEnumerable<PolicySearchResultItemReadModel> models,
            PolicyStatus status,
            BooleanQuery termsBooleanQuery)
        {
            var filterWithStatus = this.GetStatusFilter(booleanReadModelFilter, status.Humanize(), PolicyLuceneFieldNames.FieldPolicyState);
            var searchResult = searcher.Search(termsBooleanQuery, filterWithStatus, hitsCap, sort);
            return Enumerable.Concat(models, this.GetSearchResultReadModels(searcher, searchResult));
        }
    }
}
