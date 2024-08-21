// <copyright file="BaseLuceneRepository.cs" company="uBind">
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
    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.Queries;
    using Lucene.Net.QueryParsers.Classic;
    using Lucene.Net.Search;
    using Lucene.Net.Util;
    using Microsoft.Extensions.Caching.Memory;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Search;

    public abstract class BaseLuceneRepository<TItemWriteModel, TItemSearchResultModel> : SearchLuceneFacade
        where TItemWriteModel : IEntitySearchIndexWriteModel
    {
        // Since we are getting the QuoteStatus on the fly now because of the expired quotes,
        // We set this value so that we have enough values to filter out later.
        protected const int DefaultHitsCap = 10000;

        private readonly ILuceneDocumentBuilder<TItemWriteModel> luceneDocumentBuilder;
        private readonly IClock clock;
        private readonly ILuceneIndexCache luceneIndexCache;
        private readonly ICachingResolver cachingResolver;

        public BaseLuceneRepository(
            ILuceneDirectoryConfiguration luceneConfig,
            ILuceneDocumentBuilder<TItemWriteModel> luceneDocumentBuilder,
            ILuceneIndexCache luceneIndexCache,
            LuceneIndexType luceneIndexType,
            ICachingResolver cachingResolver,
            IClock clock)
            : base(luceneConfig, luceneIndexType)
        {
            this.luceneDocumentBuilder = luceneDocumentBuilder;
            this.luceneIndexCache = luceneIndexCache;
            this.cachingResolver = cachingResolver;
            this.clock = clock;
        }

        protected abstract Dictionary<string, string> DateFilteringPropertyNameToLuceneFieldMap { get; }

        protected abstract Dictionary<string, (string, SortFieldType)> SortingPropertyNameToLuceneFieldMap { get; }

        protected void UpsertDocumentsToLuceneIndex(
            DirectoryInfo indexDirectory, Dictionary<Guid, Document> documentsDictionary)
        {
            using (var writer = this.CreateIndexWriter(indexDirectory))
            {
                foreach (var keyValuePairLuceneDocument in documentsDictionary)
                {
                    try
                    {
                        var entityTerm = this.GetTermForEntityId(keyValuePairLuceneDocument.Key);
                        this.AddOrUpdateDocumentToLuceneIndex(keyValuePairLuceneDocument.Value, entityTerm, writer);
                    }
                    catch (Exception ex)
                    {
                        documentsDictionary.Clear();
                        throw new LuceneIndexingException(
                            string.Format(
                                $"There is a problem updating the lucene index for {this.LuceneIndexType.Humanize()}: " +
                                "{0}.",
                                JsonConvert.SerializeObject(keyValuePairLuceneDocument.Value)),
                            ex);
                    }
                }

                writer?.Dispose();
            }

            documentsDictionary.Clear();
        }

        /// <summary>
        /// This method will generate a filter for the productIds but as a non-tokenised / non-analysed field.
        /// This will ensure that the search is for exact match of the productIds. As an example
        /// Example:
        /// NON ANALYZED FIELD: "910db052-b22c-42cd-a4c7-094934d56e87" will be searched as "910db052-b22c-42cd-a4c7-094934d56e87"
        /// ANALYZED FIELD: "910db052-b22c-42cd-a4c7-094934d56e87" will be searched as "910db052" or
        /// "b22c" or "42cd" or "a4c7" or "094934d56e87" - which is not what we want because this is dependent on boosting and scoring.
        /// </summary>
        /// <param name="filters">QuoteReadModelFilter object.</param>
        /// <param name="filterClause">FilterClause list.</param>
        protected void GenerateNonTokenizedFilterForProducts(QuoteReadModelFilters filters, List<FilterClause> filterClause)
        {
            if (filters.ProductIds.Any())
            {
                foreach (var p in filters.ProductIds)
                {
                    // This is introduced since the field is defined as analyzed by default, and we need to search for exact match.
                    var queryParser = new QueryParser(LuceneVersion.LUCENE_48, QuoteLuceneFieldsNames.FieldProductId, this.Analyzer);
                    var query = queryParser.Parse(p.ToString());
                    filterClause.Add(new FilterClause(new QueryWrapperFilter(query), Occur.SHOULD));
                }
            }
        }

        protected Dictionary<Guid, Document> GenerateEntityLuceneDocuments(
            IEnumerable<TItemWriteModel> entityWriteModelList)
        {
            var currentDateTime = this.clock.GetCurrentInstant().ToUnixTimeTicks();

            // To avoid updating the the same document(with same Id), we use a dictionary so that a newer document
            // would overwrite the first.
            Dictionary<Guid, Document> documentsDictionary = new Dictionary<Guid, Document>();
            foreach (var entityWriteModel in entityWriteModelList)
            {
                var luceneDocument = this.luceneDocumentBuilder.BuildLuceneDocument(
                    entityWriteModel, currentDateTime);
                documentsDictionary.Add(entityWriteModel.Id, luceneDocument);
            }

            return documentsDictionary;
        }

        protected long? GetEntityIndexLastUpdatedTimestamp(
            Tenant tenant,
            DeploymentEnvironment environment,
            string lastUpdatedTimestampField,
            string lastUpdatedByUserTimestampField = null)
        {
            var key = this.GenerateLastUpdatedIndexCacheKey(tenant, environment, lastUpdatedTimestampField);
            long lastUpdated = 0;
            bool found = this.luceneIndexCache.MemoryCache.TryGetValue<long>(key, out lastUpdated);
            if (!found)
            {
                try
                {
                    lastUpdated = this.GetDocumentIndexLastUpdatedTimestamp(tenant, environment, lastUpdatedTimestampField, lastUpdatedByUserTimestampField);
                }

                // This just means that there are no lucene indexes found in the directory for this tenant and environment
                // so we just return null and just get ALL the data of the entity (without the lastUpdatedDate filter) from DB
                catch (IndexOutOfRangeException)
                {
                    return null;
                }
                catch (FileNotFoundException ex) when
                    (ex.Message.StartsWith("no segments* file found")
                        || ex.Message.Contains("Could not load file"))
                {
                    return null;
                }
            }

            return lastUpdated;
        }

        protected void MakeEntityRegenerationIndexTheLiveIndex(Tenant tenant, DeploymentEnvironment environment)
        {
            this.MakeRegenerationIndexTheLiveIndex(this.GetSuffixPath(tenant.Details.Alias, environment));
        }

        protected void MakeSureEntityRegenerationFolderIsEmptyBeforeRegeneration(Tenant tenant, DeploymentEnvironment environment)
        {
            this.DeleteOldRegenerationIndexDirectories(this.GetSuffixPath(tenant.Details.Alias, environment));
        }

        protected DirectoryInfo GetLatestLiveIndexDirectory(Tenant tenant, DeploymentEnvironment environment)
        {
            return this.GetLatestLiveIndexDirectory(this.GetSuffixPath(tenant.Details.Alias, environment));
        }

        protected DirectoryInfo CreateNewLuceneDirectory(Tenant tenant, DeploymentEnvironment environment)
        {
            return this.CreateNewLuceneDirectory(this.GetSuffixPath(tenant.Details.Alias, environment));
        }

        protected DirectoryInfo GetOrCreateRegenerationIndexDirectory(Tenant tenant, DeploymentEnvironment environment)
        {
            var suffixPath = this.GetSuffixPath(tenant.Details.Alias, environment);
            var dir = this.GetLatestRegenerationIndexDirectory(suffixPath);
            return dir ?? this.CreateRegenerationIndexDirectory(suffixPath);
        }

        protected void DeleteEntityItemsFromIndex(Tenant tenant, DeploymentEnvironment environment, IEnumerable<Guid> entityIds)
        {
            DirectoryInfo indexDirectory = this.GetLatestLiveIndexDirectory(tenant, environment);
            if (indexDirectory == null)
            {
                return;
            }

            using (var writer = this.CreateIndexWriter(indexDirectory))
            {
                if (entityIds == null || !entityIds.Any())
                {
                    throw new ErrorException(Errors.SearchIndex.DeleteItemsFromIndexNoIdsProvided());
                }

                var termsToDelete = entityIds
                    .Select(id => this.GetTermForEntityId(id)).ToArray();
                writer.DeleteDocuments(termsToDelete);
                writer.Commit();
                writer?.Dispose();
            }
        }

        protected BooleanQuery GetEntitySearchTermsQuery(EntityListFilters filters, MultiFieldQueryParser queryParser)
        {
            BooleanQuery searchTermsBooleanQuery = new BooleanQuery();
            foreach (var term in filters.SearchTerms)
            {
                searchTermsBooleanQuery.Add(queryParser.Parse($"+{QueryParser.Escape(term)}*"), Occur.MUST);
            }

            return searchTermsBooleanQuery;
        }

        protected FilterClause? GetProductFilterClause(EntityListFilters filters, string filterField)
        {
            if (filters.ProductIds.Any())
            {
                var terms = new List<Term>();
                foreach (var productId in filters.ProductIds)
                {
                    terms.Add(new Term(filterField, productId.ToString()));
                }

                var termsFilter = new TermsFilter(terms);
                return new FilterClause(termsFilter, Occur.MUST);
            }

            return null;
        }

        protected FilterClause? GetOrganisationFilterClause(EntityListFilters filters, string filterField, Occur occur = Occur.MUST)
        {
            bool hasOrganisationFilter = filters.OrganisationIds != null && filters.OrganisationIds.Any();
            if (hasOrganisationFilter)
            {
                var terms = new List<Term>();
                foreach (var organisationId in filters.OrganisationIds)
                {
                    terms.Add(new Term(filterField, organisationId.ToString("N")));
                }

                var termsFilter = new TermsFilter(terms);
                return new FilterClause(termsFilter, occur);
            }

            return null;
        }

        protected FilterClause? GetEntityGuidFieldFilterClause(Guid? id, string filterField)
        {
            if (id.HasValue && id.Value != Guid.Empty)
            {
                var filter = new TermsFilter(new Term(filterField, id.Value.ToString("N")));
                return new FilterClause(filter, Occur.MUST);
            }

            return null;
        }

        protected FilterClause? GetDateRangeFilterClause(EntityListFilters filters)
        {
            if (filters.DateFilteringPropertyName == null || (filters.DateIsBeforeTicks == null && filters.DateIsAfterTicks == null))
            {
                return null;
            }

            long maxDate;
            long minDate = 0;

            if (!this.DateFilteringPropertyNameToLuceneFieldMap.TryGetValue(filters.DateFilteringPropertyName, out string? filterByDate))
            {
                throw new ErrorException(Errors.SearchIndex.InvalidDateFilteringPropertyName(filters.DateFilteringPropertyName));
            }

            if (filters.DateIsBeforeTicks != null)
            {
                maxDate = (long)filters.DateIsBeforeTicks;
            }
            else
            {
                maxDate = Instant.MaxValue.ToUnixTimeTicks();
            }

            if (filters.DateIsAfterTicks != null)
            {
                minDate = (long)filters.DateIsAfterTicks;
            }

            var dateRangeFilter = NumericRangeFilter.NewInt64Range(filterByDate, minDate, maxDate, false, false);
            return new FilterClause(dateRangeFilter, Occur.MUST);
        }

        protected FilterClause GetBooleanFieldFilterClause(bool value, string filterField)
        {
            var termsFilter = new TermsFilter(new Term(filterField, value.ToString().ToLower()));
            return new FilterClause(termsFilter, Occur.MUST);
        }

        protected BooleanFilter GenerateBooleanEntityReadModelFilter(List<FilterClause> filterClause)
        {
            BooleanFilter booleanFilter = new BooleanFilter();
            if (!filterClause.Any(c => c != null))
            {
                return null;
            }

            foreach (var filter in filterClause.Where(c => c != null))
            {
                booleanFilter.Add(filter);
            }

            return booleanFilter;
        }

        protected BooleanFilter GetStatusFilter(BooleanFilter baseFilter, string status, string fieldName)
        {
            var statusFilter = baseFilter ?? new BooleanFilter();
            var termsFilter = new TermsFilter(new Term(fieldName, status.ToLower()));
            statusFilter.Add(new FilterClause(termsFilter, Occur.MUST));
            return statusFilter;
        }

        protected Sort GetEntitySorting(string sortByFieldName, SortFieldType sortFieldType, SortDirection sortOrder)
        {
            bool isDescending = sortOrder == SortDirection.Descending;
            SortField sortField = new SortField(sortByFieldName, sortFieldType, isDescending);
            return new Sort(sortField);
        }

        protected TopFieldDocs SearchAllEntityDocuments(
            IndexSearcher searcher, Sort sort, int hitsCap, BooleanFilter booleanFilter, BooleanQuery searchTermsBooleanQuery = null)
        {
            if (searchTermsBooleanQuery != null)
            {
                return searcher.Search(searchTermsBooleanQuery, booleanFilter, hitsCap, sort);
            }
            else
            {
                return searcher.Search(new MatchAllDocsQuery(), booleanFilter, hitsCap, sort);
            }
        }

        protected IEnumerable<TItemSearchResultModel> GetEntityListInRange(
            IEnumerable<TItemSearchResultModel> searchResults, EntityListFilters filters)
        {
            int totalNumberOfSearchResults = searchResults.Count();
            int pageSize = filters.PageSize.HasValue ? filters.PageSize.Value : (int)PageSize.Default;
            int page = filters.Page.HasValue ? filters.Page.Value : 1;
            int start = pageSize * (page - 1);
            int amount = Math.Min(totalNumberOfSearchResults - start, pageSize);
            return searchResults.ToList().GetRange(start, amount);
        }

        protected string GenerateLastUpdatedIndexCacheKey(Tenant tenant, DeploymentEnvironment environment, string lastUpdatedTimestampField)
        {
            return string.Join("|---|", new string[] { tenant.Id.ToString(), environment.ToString(), this.LuceneIndexType.Humanize(), lastUpdatedTimestampField });
        }

        protected void RemoveIndexLastUpdatedTimestampCache(Tenant tenant, DeploymentEnvironment environment, string lastUpdatedTimestampField)
        {
            var key = this.GenerateLastUpdatedIndexCacheKey(tenant, environment, lastUpdatedTimestampField);
            this.luceneIndexCache.MemoryCache.Remove(key);
        }

        protected void SetIndexLastUpdatedIndexCacheKey(
            Tenant tenant,
            DeploymentEnvironment environment,
            string lastUpdatedTimestampField,
            string lastUpdatedByUserTimestampField = null)
        {
            var key = this.GenerateLastUpdatedIndexCacheKey(tenant, environment, lastUpdatedTimestampField);
            long lastUpdated = this.GetDocumentIndexLastUpdatedTimestamp(tenant, environment, lastUpdatedTimestampField, lastUpdatedByUserTimestampField);
            this.luceneIndexCache.MemoryCache.Set<long>(key, lastUpdated, this.luceneIndexCache.MemoryCacheEntryOptions);
        }

        protected long GetDocumentIndexLastUpdatedTimestamp(
            Tenant tenant,
            DeploymentEnvironment environment,
            string lastUpdatedTimestampField,
            string lastUpdatedByUserTimestampField = null)
        {
            long lastUpdated = 0;
            var indexDirectory = this.GetLatestLiveIndexDirectory(tenant, environment);
            if (indexDirectory == null)
            {
                return lastUpdated;
            }

            var searcher = this.CreateIndexSearcher(indexDirectory);
            var sortField = lastUpdatedByUserTimestampField != null ? lastUpdatedByUserTimestampField : lastUpdatedTimestampField;
            var sort = this.GetEntitySorting(sortField, SortFieldType.INT64, SortDirection.Descending);
            var results = searcher.Search(new MatchAllDocsQuery(), null, 1, sort);

            if (results.ScoreDocs.Length > 0)
            {
                var document = searcher.Doc(results.ScoreDocs[0].Doc);
                long.TryParse(document.Get(lastUpdatedTimestampField), out lastUpdated);
                if (lastUpdatedByUserTimestampField != null)
                {
                    long.TryParse(document.Get(lastUpdatedByUserTimestampField), out var lastUpdatedByUser);
                    return lastUpdated > lastUpdatedByUser ? lastUpdated : lastUpdatedByUser;
                }
            }

            return lastUpdated;
        }

        protected int GetLuceneIndexCountBetweenDates(Tenant tenant, DeploymentEnvironment environment, EntityListFilters filters, string sortField)
        {
            DirectoryInfo indexDirectory = this.GetLatestLiveIndexDirectory(tenant, environment);
            if (indexDirectory == null || !indexDirectory.Exists)
            {
                return 0;
            }

            // this is the max count to retrieve this was required property so I set it initially to 5000. but we can adjust this.
            int hitsCap = 5000;

            var searcher = this.CreateIndexSearcher(indexDirectory);

            BooleanFilter datesBooleanFilter = new BooleanFilter();
            datesBooleanFilter.Add(this.GetDateRangeFilterClause(filters));
            Sort sort = this.GetEntitySorting(sortField, SortFieldType.INT64, SortDirection.Descending);
            TopFieldDocs searchResult = this.SearchAllEntityDocuments(searcher, sort, hitsCap, datesBooleanFilter);
            return searchResult.ScoreDocs.Length;
        }

        private string GetSuffixPath(string tenantAlias, DeploymentEnvironment environment)
        {
            return string.Format(@"{0}\{1}", tenantAlias, environment);
        }

        private async Task<string?> GetTenantAlias(Guid tenantId)
        {
            var tenant = await this.cachingResolver.GetTenantOrThrow(tenantId);
            return tenant?.Details?.Alias;
        }

        private Term GetTermForEntityId(Guid entityId)
        {
            switch (this.LuceneIndexType)
            {
                case LuceneIndexType.Quote:
                    return new Term(QuoteLuceneFieldsNames.FieldQuoteId, entityId.ToString("N"));
                case LuceneIndexType.Policy:
                    return new Term(PolicyLuceneFieldNames.FieldPolicyId, entityId.ToString("N"));
                default:
                    return new Term("defaultid", entityId.ToString("N"));
            }
        }
    }
}
