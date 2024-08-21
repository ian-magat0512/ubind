// <copyright file="LuceneQuoteRepository.cs" company="uBind">
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
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// A repository which stores and reads quotes from a lucene search index.
    /// </summary>
    public class LuceneQuoteRepository : BaseLuceneRepository<IQuoteSearchIndexWriteModel, IQuoteSearchResultItemReadModel>,
        ILuceneRepository<IQuoteSearchIndexWriteModel, IQuoteSearchResultItemReadModel, QuoteReadModelFilters>
    {
        private readonly ILogger<LuceneQuoteRepository> logger;
        private readonly string lastUpdatedTimestampField = QuoteLuceneFieldsNames.FieldLastUpdatedTimeStamp;
        private readonly string lastUpdatedByUserTimestampField = QuoteLuceneFieldsNames.FieldLastUpdatedByUserTimestamp;
        private readonly Dictionary<string, DocStatus> quoteStatesMapping = new Dictionary<string, DocStatus>
        {
            { StandardQuoteStates.Incomplete.ToLower(), DocStatus.NonExpired },
            { StandardQuoteStates.Review.ToLower(), DocStatus.NonExpired },
            { StandardQuoteStates.Endorsement.ToLower(), DocStatus.NonExpired },
            { StandardQuoteStates.Approved.ToLower(), DocStatus.NonExpired },
            { StandardQuoteStates.Complete.ToLower(), DocStatus.All },
            { StandardQuoteStates.Declined.ToLower(), DocStatus.All },
            { StandardQuoteStates.Expired.ToLower(), DocStatus.Expired },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="LuceneQuoteRepository"/> class.
        /// </summary>
        /// <param name="luceneConfig">The lucene directory Configuration.</param>
        /// <param name="luceneDocumentBuilder">The lucene document builder.</param>
        /// <param name="clock">Nodatime clock for date stamping.</param>
        /// <param name="luceneIndexCache">Lucene quote memory cache.</param>
        /// <param name="logger">The logger.</param>
        public LuceneQuoteRepository(
            ILuceneDirectoryConfiguration luceneConfig,
            ILuceneDocumentBuilder<IQuoteSearchIndexWriteModel> luceneDocumentBuilder,
            IClock clock,
            ILuceneIndexCache luceneIndexCache,
            ICachingResolver cachingResolver,
            ILogger<LuceneQuoteRepository> logger)
            : base(luceneConfig, luceneDocumentBuilder, luceneIndexCache, LuceneIndexType.Quote, cachingResolver, clock)
        {
            this.logger = logger;
            this.Analyzer = new CustomLuceneAnalyzer();
        }

        /// <summary>
        /// Doc status enum.
        /// </summary>
        private enum DocStatus
        {
            /// <summary>
            /// Expired docs
            /// </summary>
            Expired,

            /// <summary>
            /// Non expired docs
            /// </summary>
            NonExpired,

            /// <summary>
            /// All docs.
            /// </summary>
            All,
        }

        public override Analyzer Analyzer
        {
            get { return base.Analyzer; }
            set { base.Analyzer = value; }
        }

        protected override Dictionary<string, string> DateFilteringPropertyNameToLuceneFieldMap =>
           new Dictionary<string, string>
           {
                { nameof(PolicySearchResultItemReadModel.CreatedTicksSinceEpoch), QuoteLuceneFieldsNames.FieldCreatedTimestamp },
                { nameof(PolicySearchResultItemReadModel.LastModifiedTicksSinceEpoch), QuoteLuceneFieldsNames.FieldLastUpdatedTimeStamp },
                { nameof(PolicySearchResultItemReadModel.ExpiryTicksSinceEpoch), QuoteLuceneFieldsNames.FieldExpiryTimestamp },
           };

        protected override Dictionary<string, (string, SortFieldType)> SortingPropertyNameToLuceneFieldMap =>
            new Dictionary<string, (string, SortFieldType)>
            {
                { nameof(PolicySearchResultItemReadModel.CreatedTicksSinceEpoch), (QuoteLuceneFieldsNames.FieldCreatedTimestamp, SortFieldType.INT64) },
                { nameof(PolicySearchResultItemReadModel.LastModifiedTicksSinceEpoch), (QuoteLuceneFieldsNames.FieldLastUpdatedTimeStamp, SortFieldType.INT64) },
                { nameof(PolicySearchResultItemReadModel.ExpiryTicksSinceEpoch), (QuoteLuceneFieldsNames.FieldExpiryTimestamp, SortFieldType.INT64) },
                { nameof(PolicySearchResultItemReadModel.CustomerFullName), (QuoteLuceneFieldsNames.FieldCustomerFullName, SortFieldType.STRING) },
            };

        private MultiFieldQueryParser QuotesQueryParser
        {
            get
            {
                var searchFields = new string[]
                {
                    QuoteLuceneFieldsNames.FieldFormDataJsonValues,
                    QuoteLuceneFieldsNames.FieldQuoteNumber,
                    QuoteLuceneFieldsNames.FieldQuoteTitle,
                    QuoteLuceneFieldsNames.FieldCustomerFullName,
                    QuoteLuceneFieldsNames.FieldCustomerPreferredName,
                    QuoteLuceneFieldsNames.FieldCustomerEmail,
                    QuoteLuceneFieldsNames.FieldCustomerAlternativeEmail,
                    QuoteLuceneFieldsNames.FieldCustomerHomePhone,
                    QuoteLuceneFieldsNames.FieldCustomerMobilePhone,
                    QuoteLuceneFieldsNames.FieldWorkPhone,
                    QuoteLuceneFieldsNames.FieldQuoteType,
                    QuoteLuceneFieldsNames.FieldOwnerFullname,
                    QuoteLuceneFieldsNames.FieldProductId,
                    QuoteLuceneFieldsNames.FieldPolicyNumber,
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
        public int GetEntityIndexCountBetweenDates(
            Tenant tenant,
            DeploymentEnvironment environment,
            Instant fromDateTime,
            Instant toDateTime)
        {
            var filters = new EntityListFilters
            {
                DateFilteringPropertyName = nameof(QuoteSearchResultItemReadModel.LastModifiedTicksSinceEpoch),
                DateIsAfterTicks = fromDateTime.ToUnixTimeTicks(),
                DateIsBeforeTicks = toDateTime.ToUnixTimeTicks(),
            };

            return this.GetLuceneIndexCountBetweenDates(tenant, environment, filters, this.lastUpdatedTimestampField);
        }

        /// <inheritdoc/>
        public IEnumerable<IQuoteSearchResultItemReadModel> Search(
            Tenant tenant,
            DeploymentEnvironment environment,
            QuoteReadModelFilters filters)
        {
            if (!this.SortingPropertyNameToLuceneFieldMap.TryGetValue(filters.SortBy, out (string, SortFieldType) sortByLuceneFieldResult))
            {
                throw new ErrorException(Errors.SearchIndex.InvalidSortingPropertyName(filters.SortBy));
            }

            var (sortByLuceneField, sortFieldType) = sortByLuceneFieldResult;
            Sort sort = this.GetEntitySorting(sortByLuceneField, sortFieldType, filters.SortOrder);
            TopFieldDocs searchResult;

            var searchResults = Enumerable.Empty<QuoteSearchResultItemReadModel>();
            DirectoryInfo indexDirectory = this.GetLatestLiveIndexDirectory(tenant, environment);
            if (indexDirectory == null || !indexDirectory.Exists)
            {
                // no quotes have been indexed, since we don't even have an index directory yet.
                this.logger.LogInformation($"Lucene index directory was not found. It's likely that no quotes have been indexed "
                    + $" yet in this environment. Tenant={tenant.Id}, Environment={environment}.");
                return searchResults;
            }

            var searcher = this.CreateIndexSearcher(indexDirectory);

            BooleanQuery searchTermsBooleanQuery = null;

            if (filters.SearchTerms.Any())
            {
                searchTermsBooleanQuery = this.GetEntitySearchTermsQuery(filters, this.QuotesQueryParser);
            }

            if (!filters.Statuses.Any())
            {
                BooleanFilter entityFieldFilter = this.GenerateEntityFieldFilter(filters);
                searchResult = searcher.Search(searchTermsBooleanQuery, entityFieldFilter, DefaultHitsCap, sort);
                searchResults = Enumerable.Concat(searchResults, this.GetSearchResultReadModels(searcher, searchResult));

                if (filters.IsRideProtectOrganisation)
                {
                    BooleanFilter entityFieldFilterForRideProtect =
                        this.GenerateEntityFieldFilterForRideProtectProductFromOtherOrganisation(filters);
                    var searchResultForRideProtect = searcher.Search(searchTermsBooleanQuery, entityFieldFilterForRideProtect, DefaultHitsCap, sort);
                    searchResults = Enumerable.Concat(searchResults, this.GetSearchResultReadModels(searcher, searchResultForRideProtect));
                }
            }
            else
            {
                // If there are no Quote status filters but there are other filters e.g. date, return all Quote Types
                if (!filters.Statuses.Any())
                {
                    filters.Statuses = new List<string>()
                    {
                        StandardQuoteStates.Incomplete.ToString(),
                        StandardQuoteStates.Complete.ToString(),
                        StandardQuoteStates.Declined.ToString(),
                        StandardQuoteStates.Approved.ToString(),
                        StandardQuoteStates.Nascent.ToString(),
                        StandardQuoteStates.Review.ToString(),
                        StandardQuoteStates.Expired.ToString(),
                        StandardQuoteStates.Endorsement.ToString(),
                    };
                }

                var filterStatuses = filters.Statuses.Where(s => this.quoteStatesMapping.ContainsKey(s.ToLower()));
                foreach (var status in filterStatuses)
                {
                    var now = SystemClock.Instance.Now();
                    var quoteStatus = this.quoteStatesMapping[status.ToLower()];

                    // This needs to be reset each loop as BooleanFilter is a ref type.
                    BooleanFilter entityFieldFilter = this.GenerateEntityFieldFilter(filters);

                    if (quoteStatus == DocStatus.NonExpired)
                    {
                        if (searchTermsBooleanQuery != null)
                        {
                            searchResults = this.GetNonExpiredQuotesWithTerm(
                                searcher, sort, entityFieldFilter, DefaultHitsCap, searchResults, status, now, searchTermsBooleanQuery);
                        }
                        else
                        {
                            searchResults = this.GetNonExpiredQuotes(
                                searcher, sort, entityFieldFilter, DefaultHitsCap, searchResults, status, now);
                        }
                    }
                    else if (quoteStatus == DocStatus.All)
                    {
                        searchResults = this.GetAllQuotes(
                            searcher, sort, entityFieldFilter, DefaultHitsCap, searchResults, status, searchTermsBooleanQuery);
                    }
                    else if (quoteStatus == DocStatus.Expired)
                    {
                        searchResults = this.GetExpiredQuotes(
                            searcher, sort, entityFieldFilter, DefaultHitsCap, searchResults, now, searchTermsBooleanQuery);
                    }

                    if (filters.IsRideProtectOrganisation)
                    {
                        BooleanFilter entityFieldFilterForRideProtect =
                        this.GenerateEntityFieldFilterForRideProtectProductFromOtherOrganisation(filters);

                        if (quoteStatus == DocStatus.NonExpired)
                        {
                            if (searchTermsBooleanQuery != null)
                            {
                                searchResults = this.GetNonExpiredQuotesWithTerm(
                                    searcher, sort, entityFieldFilterForRideProtect, DefaultHitsCap, searchResults, status, now, searchTermsBooleanQuery);
                            }
                            else
                            {
                                searchResults = this.GetNonExpiredQuotes(
                                    searcher, sort, entityFieldFilterForRideProtect, DefaultHitsCap, searchResults, status, now);
                            }
                        }
                        else if (quoteStatus == DocStatus.All)
                        {
                            searchResults = this.GetAllQuotes(
                                searcher, sort, entityFieldFilterForRideProtect, DefaultHitsCap, searchResults, status, searchTermsBooleanQuery);
                        }
                        else if (quoteStatus == DocStatus.Expired)
                        {
                            searchResults = this.GetExpiredQuotes(
                                searcher, sort, entityFieldFilterForRideProtect, DefaultHitsCap, searchResults, now, searchTermsBooleanQuery);
                        }
                    }
                }
            }

            searcher.IndexReader.Dispose();
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
            IEnumerable<IQuoteSearchIndexWriteModel> quotes)
        {
            Dictionary<Guid, Document> documentsDictionary = this.GenerateEntityLuceneDocuments(quotes);
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
            IEnumerable<IQuoteSearchIndexWriteModel> quotes)
        {
            var documentsDictionary = this.GenerateEntityLuceneDocuments(quotes);
            var indexDirectory = this.GetOrCreateRegenerationIndexDirectory(tenant, environment);
            this.UpsertDocumentsToLuceneIndex(indexDirectory, documentsDictionary);
            this.RemoveIndexLastUpdatedTimestampCache(tenant, environment, this.lastUpdatedTimestampField);
        }

        /// <inheritdoc/>
        public void DeleteItemsFromIndex(Tenant tenant, DeploymentEnvironment environment, IEnumerable<Guid> quoteIds)
        {
            this.DeleteEntityItemsFromIndex(tenant, environment, quoteIds);
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

        private IEnumerable<QuoteSearchResultItemReadModel> GetExpiredQuotes(
            IndexSearcher searcher,
            Sort sort,
            BooleanFilter booleanReadModelFilter,
            int hitsCap,
            IEnumerable<QuoteSearchResultItemReadModel> models,
            Instant now,
            BooleanQuery searchTermsBooleanQuery = null)
        {
            var statusFilter = this.GetStatusFilter(booleanReadModelFilter, StandardQuoteStates.Expired, QuoteLuceneFieldsNames.FieldQuoteState);
            var searchResult = this.SearchWithinExpiredQuotes(searcher, sort, statusFilter, hitsCap, now, searchTermsBooleanQuery);
            return Enumerable.Concat(models, this.GetSearchResultReadModels(searcher, searchResult));
        }

        private IEnumerable<QuoteSearchResultItemReadModel> GetAllQuotes(
            IndexSearcher searcher,
            Sort sort,
            BooleanFilter entityFieldFilter,
            int hitsCap,
            IEnumerable<QuoteSearchResultItemReadModel> models,
            string status,
            BooleanQuery searchTermsBooleanQuery = null)
        {
            var filterWithStatus = this.GetStatusFilter(entityFieldFilter, status, QuoteLuceneFieldsNames.FieldQuoteState);
            var searchResult = this.SearchAllEntityDocuments(searcher, sort, hitsCap, filterWithStatus, searchTermsBooleanQuery);
            return Enumerable.Concat(models, this.GetSearchResultReadModels(searcher, searchResult));
        }

        private IEnumerable<QuoteSearchResultItemReadModel> GetNonExpiredQuotes(
            IndexSearcher searcher,
            Sort sort,
            BooleanFilter booleanReadModelFilter,
            int hitsCap,
            IEnumerable<QuoteSearchResultItemReadModel> models,
            string status,
            Instant now)
        {
            var filterWithStatus = this.GetStatusFilter(booleanReadModelFilter, status, QuoteLuceneFieldsNames.FieldQuoteState);
            var searchResult = this.SearchExcludingExpiredQuotes(searcher, sort, hitsCap, filterWithStatus, now);
            models = Enumerable.Concat(models, this.GetSearchResultReadModels(searcher, searchResult));

            searchResult = this.SearchExcludingQuotesWithExpiryDate(searcher, sort, hitsCap, filterWithStatus);
            return Enumerable.Concat(models, this.GetSearchResultReadModels(searcher, searchResult));
        }

        private IEnumerable<QuoteSearchResultItemReadModel> GetNonExpiredQuotesWithTerm(
            IndexSearcher searcher,
            Sort sort,
            BooleanFilter entityFieldFilter,
            int hitsCap,
            IEnumerable<QuoteSearchResultItemReadModel> searchResults,
            string status,
            Instant now,
            BooleanQuery searchTermsBooleanQuery)
        {
            var booleanFilter = this.GetStatusFilter(entityFieldFilter, status, QuoteLuceneFieldsNames.FieldQuoteState);
            this.SetNoExpiryDateFilter(booleanFilter);
            var searchResult = this.SearchExcludingExpiredQuotes(searcher, sort, hitsCap, booleanFilter, now, searchTermsBooleanQuery);
            searchResults = Enumerable.Concat(searchResults, this.GetSearchResultReadModels(searcher, searchResult));
            return searchResults;
        }

        private BooleanFilter GenerateEntityFieldFilter(QuoteReadModelFilters filters)
        {
            List<FilterClause> filterClause = new List<FilterClause>()
            {
                this.GetQuotesTypesFilterClause(filters),
                this.GetDateRangeFilterClause(filters),
                this.GetEntityGuidFieldFilterClause(filters.CustomerId, QuoteLuceneFieldsNames.FieldCustomerId),
                this.GetOrganisationFilterClause(filters, QuoteLuceneFieldsNames.FieldOrganisationId),
                this.GetEntityGuidFieldFilterClause(filters.OwnerUserId, QuoteLuceneFieldsNames.FieldOwnerUserId),
            };

            this.GenerateNonTokenizedFilterForProducts(filters, filterClause);
            filterClause.Add(this.GetBooleanFieldFilterClause(false, QuoteLuceneFieldsNames.IsDiscarded));

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
        private BooleanFilter GenerateEntityFieldFilterForRideProtectProductFromOtherOrganisation(QuoteReadModelFilters filters)
        {
            var productsFilter = new TermsFilter(new Term(QuoteLuceneFieldsNames.FieldProductId, filters.RideProtectProductId.ToString()));
            var productFilterClause = new FilterClause(productsFilter, Occur.MUST);
            var organisationFilterClause = this.GetOrganisationFilterClause(filters, QuoteLuceneFieldsNames.FieldOrganisationId, Occur.MUST_NOT);
            List<FilterClause> filterClause = new List<FilterClause>()
            {
                productFilterClause,
                organisationFilterClause,
                this.GetQuotesTypesFilterClause(filters),
                this.GetDateRangeFilterClause(filters),
                this.GetEntityGuidFieldFilterClause(filters.CustomerId, QuoteLuceneFieldsNames.FieldCustomerId),
                this.GetEntityGuidFieldFilterClause(filters.OwnerUserId, QuoteLuceneFieldsNames.FieldOwnerUserId),
            };

            if (!filters.SearchTerms.Any() && !filters.IncludeTestData)
            {
                filterClause.Add(this.GetBooleanFieldFilterClause(filters.IncludeTestData, QuoteLuceneFieldsNames.IsTestData));
            }

            return this.GenerateBooleanEntityReadModelFilter(filterClause);
        }

        private BooleanFilter SetNoExpiryDateFilter(BooleanFilter booleanFilter)
        {
            var dateRangeFilter = NumericRangeFilter.NewInt64Range(
                QuoteLuceneFieldsNames.FieldExpiryTimestamp,
                Instant.MinValue.ToUnixTimeTicks(),
                Instant.MaxValue.ToUnixTimeTicks(),
                false,
                false);
            booleanFilter.Add(new FilterClause(dateRangeFilter, Occur.MUST_NOT));
            return booleanFilter;
        }

        private IEnumerable<QuoteSearchResultItemReadModel> GetSearchResultReadModels(
            IndexSearcher searcher, TopFieldDocs searchResults)
        {
            var results = new List<QuoteSearchResultItemReadModel>();

            foreach (var documentNumber in searchResults.ScoreDocs.Select(s => s.Doc))
            {
                var documentToParse = searcher.Doc(documentNumber);
                var quoteId = Guid.Parse(documentToParse.Get(QuoteLuceneFieldsNames.FieldQuoteId));
                var createdTimestamp = long.Parse(documentToParse.Get(QuoteLuceneFieldsNames.FieldCreatedTimestamp));
                var lastUpdatedTimestamp = long.Parse(documentToParse.Get(QuoteLuceneFieldsNames.FieldLastUpdatedByUserTimestamp)
                    ?? documentToParse.Get(QuoteLuceneFieldsNames.FieldLastUpdatedTimeStamp));
                results.Add(new QuoteSearchResultItemReadModel(documentToParse, quoteId, createdTimestamp, lastUpdatedTimestamp));
            }

            return results;
        }

        private FilterClause GetQuotesTypesFilterClause(EntityListFilters filters)
        {
            if (filters.QuoteTypes.Any())
            {
                var filter = this.GetQuoteTypesFilter(filters);
                return new FilterClause(filter, Occur.MUST);
            }

            return null;
        }

        private TermsFilter GetQuoteTypesFilter(EntityListFilters filters)
        {
            var list = new List<Term>();
            if (filters.QuoteTypes.Any())
            {
                foreach (var type in filters.QuoteTypes)
                {
                    var quoteType = type.ToEnumOrThrow<QuoteType>();
                    var quoteTypeInt = (int)quoteType;
                    list.Add(new Term(QuoteLuceneFieldsNames.FieldQuoteType, quoteTypeInt.ToString()));
                }
            }

            return new TermsFilter(list);
        }

        private TopFieldDocs SearchWithinExpiredQuotes(
            IndexSearcher searcher,
            Sort sort,
            BooleanFilter booleanFilter,
            int hitsCap,
            Instant now,
            BooleanQuery searchTermsBooleanQuery = null)
        {
            TopFieldDocs searchResult;
            var expiredQuery = NumericRangeQuery.NewInt64Range(
                QuoteLuceneFieldsNames.FieldExpiryTimestamp,
                Instant.MinValue.ToUnixTimeTicks(),
                now.ToUnixTimeTicks(),
                true,
                true);
            if (searchTermsBooleanQuery != null)
            {
                searchTermsBooleanQuery.Add(expiredQuery, Occur.SHOULD);
                searchResult = searcher.Search(searchTermsBooleanQuery, booleanFilter, hitsCap, sort);
            }
            else
            {
                searchResult = searcher.Search(expiredQuery, booleanFilter, hitsCap, sort);
            }

            return searchResult;
        }

        private TopFieldDocs SearchExcludingExpiredQuotes(
            IndexSearcher searcher,
            Sort sort,
            int hitsCap,
            BooleanFilter booleanFilter,
            Instant now,
            BooleanQuery searchTermsBooleanQuery = null)
        {
            TopFieldDocs searchResult;
            var nonExpiredQuery = NumericRangeQuery.NewInt64Range(
                QuoteLuceneFieldsNames.FieldExpiryTimestamp,
                now.ToUnixTimeTicks(),
                Instant.MaxValue.ToUnixTimeTicks(),
                false,
                true);
            if (searchTermsBooleanQuery != null)
            {
                searchTermsBooleanQuery.Add(nonExpiredQuery, Occur.SHOULD);
                searchResult = searcher.Search(searchTermsBooleanQuery, booleanFilter, hitsCap, sort);
            }
            else
            {
                searchResult = searcher.Search(nonExpiredQuery, booleanFilter, hitsCap, sort);
            }

            return searchResult;
        }

        private TopFieldDocs SearchExcludingQuotesWithExpiryDate(
            IndexSearcher searcher, Sort sort, int hitsCap, BooleanFilter booleanFilter)
        {
            var filterWithNoExpiryDate = this.SetNoExpiryDateFilter(booleanFilter);
            TopFieldDocs searchResult = searcher.Search(new MatchAllDocsQuery(), filterWithNoExpiryDate, hitsCap, sort);
            return searchResult;
        }
    }
}
