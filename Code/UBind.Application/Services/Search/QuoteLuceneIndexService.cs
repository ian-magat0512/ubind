// <copyright file="QuoteLuceneIndexService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Search;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using global::Lucene.Net.Index;
using Humanizer;
using Microsoft.Extensions.Logging;
using NodaTime;
using UBind.Application.Helpers;
using UBind.Application.Services.Email;
using UBind.Domain;
using UBind.Domain.Enums;
using UBind.Domain.Exceptions;
using UBind.Domain.Product;
using UBind.Domain.ReadModel;
using UBind.Domain.Repositories;
using UBind.Domain.Search;

/// <inheritdoc/>
public class QuoteLuceneIndexService : BaseLuceneIndexService, ISearchableEntityService<IQuoteSearchResultItemReadModel, QuoteReadModelFilters>
{
    private readonly IQuoteReadModelRepository quoteReadModelRepository;
    private readonly ILuceneRepository<IQuoteSearchIndexWriteModel, IQuoteSearchResultItemReadModel, QuoteReadModelFilters> luceneQuoteRepository;
    private readonly ILogger<QuoteLuceneIndexService> logger;
    private readonly IErrorNotificationService errorNotificationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuoteLuceneIndexService"/> class.
    /// </summary>e
    /// <param name="quoteReadModelRepository">The quote repository.</param>
    /// <param name="tenantRepository">The tenant repository.</param>
    /// <param name="luceneQuoteRepository">The lucene index repository for quotes.</param>
    /// <param name="errorNotificationService">The error notification service.</param>
    /// <param name="logger">The logger.</param>
    public QuoteLuceneIndexService(
        IQuoteReadModelRepository quoteReadModelRepository,
        ITenantRepository tenantRepository,
        ILuceneRepository<IQuoteSearchIndexWriteModel, IQuoteSearchResultItemReadModel, QuoteReadModelFilters> luceneQuoteRepository,
        ILogger<QuoteLuceneIndexService> logger,
        IErrorNotificationService errorNotificationService)
        : base(tenantRepository, logger)
    {
        this.quoteReadModelRepository = quoteReadModelRepository;
        this.luceneQuoteRepository = luceneQuoteRepository;
        this.logger = logger;
        this.errorNotificationService = errorNotificationService;
    }

    /// <inheritdoc/>
    public void ProcessIndexUpdates(
        DeploymentEnvironment environment,
        IEnumerable<Product> products,
        CancellationToken cancellationToken)
    {
        try
        {
            this.ProcessLuceneIndexUpdatesForAllTenants(environment, products, cancellationToken);
        }
        catch (Exception exception)
        {
            this.errorNotificationService.CaptureSentryException(exception, environment);
            throw;
        }
    }

    /// <inheritdoc/>
    public IEntityRepositoryAndLuceneIndexCountModel GetRepositoryAndLuceneIndexCount(
        Tenant tenant,
        DeploymentEnvironment environment,
        Instant fromDateTime,
        Instant toDateTime)
    {
        var repositoryCount = this.quoteReadModelRepository.GetQuoteCountBetweenDates(tenant.Id, environment, fromDateTime, toDateTime);
        var luceneIndexCount = this.luceneQuoteRepository.GetEntityIndexCountBetweenDates(tenant, environment, fromDateTime, toDateTime);
        return new EntityRepositoryAndLuceneIndexCountModel(tenant.Details?.Alias, repositoryCount, luceneIndexCount);
    }

    /// <inheritdoc/>
    public IEnumerable<IQuoteSearchResultItemReadModel> Search(
        Tenant tenant,
        DeploymentEnvironment environment,
        QuoteReadModelFilters filters)
    {
        try
        {
            return this.luceneQuoteRepository.Search(tenant, environment, filters);
        }
        catch (CorruptIndexException ex)
        {
            var exception = new ErrorException(
                Errors.LuceneIndex.SearchIndexCorrupted(tenant.Details.Alias, environment, LuceneIndexType.Quote.Humanize().ToLower(), ex.Message));
            throw exception;
        }
    }

    /// <inheritdoc/>
    public void RegenerateLuceneIndexesForTenant(Tenant tenant, IEnumerable<Product> products, CancellationToken cancellationToken)
    {
        var tenants = new List<Tenant> { tenant };
        this.RegenerateLuceneIndexes(DeploymentEnvironment.Production, products, cancellationToken, tenants);
        this.RegenerateLuceneIndexes(DeploymentEnvironment.Staging, products, cancellationToken, tenants);
        this.RegenerateLuceneIndexes(DeploymentEnvironment.Development, products, cancellationToken, tenants);
    }

    /// <inheritdoc/>
    public void RegenerateLuceneIndexes(
        DeploymentEnvironment environment,
        IEnumerable<Product> products,
        CancellationToken cancellationToken,
        IEnumerable<Tenant> tenants = null)
    {
        try
        {
            if (tenants == null)
            {
                this.logger.LogInformation(
                    "Starting quote index regeneration in {0} environment for all tenants.",
                    environment.Humanize());
            }
            else
            {
                string tenantAliases = string.Join(", ", tenants.Select(t => t.Details.Alias));
                this.logger.LogInformation(
                    "Starting quote index regeneration in {0} environment for tenants: {1}.",
                    environment.Humanize(),
                    tenantAliases);
            }
            this.RegenerateEntityLuceneIndexes(environment, products, cancellationToken, tenants);
            this.logger.LogInformation(
                "Completed quote index regeneration in {0} environment.",
                environment.Humanize());
        }
        catch (Exception exception)
        {
            this.logger.LogError(
                "Failed regenerating quotes index in {0} environment: {1}",
                environment,
                exception.Message);
            this.errorNotificationService.CaptureSentryException(exception, environment);
            throw;
        }
    }

    /// <inheritdoc/>
    public override int ProcessLuceneIndexUpdates(
        Tenant tenant,
        DeploymentEnvironment environment,
        IEnumerable<Product> products,
        CancellationToken cancellationToken)
    {
        try
        {
            var indexLastUpdatedTicksSinceEpoch =
                this.luceneQuoteRepository.GetIndexLastUpdatedTicksSinceEpoch(tenant, environment);

            return this.AddNewQuotesToSearchIndexSince(
                tenant,
                environment,
                products,
                cancellationToken,
                indexLastUpdatedTicksSinceEpoch);
        }
        catch (Exception exception)
        {
            this.logger.LogError(
                "Failed updating the quotes index for tenant {0} in environment {1}: {2}",
                tenant.Details.Alias,
                environment.Humanize(),
                exception.Message);

            var additionalDetails = this.errorNotificationService.GetErrorDetails(
                tenant.Details.Alias,
                environment.Humanize(),
                exception,
                $"The exception originated from: {LuceneIndexType.Quote.Humanize()}");
            this.errorNotificationService.CaptureSentryException(exception, environment, additionalDetails);
        }

        return 0;
    }

    public override int AddEntitiesToLuceneRegenerationIndex(
        Tenant tenant,
        DeploymentEnvironment environment,
        IEnumerable<Product> products,
        CancellationToken cancellationToken)
    {
        this.logger.LogInformation(
            "Quote index regeneration for tenant {0} in environment {1}: Adding quotes to the regeneration index.",
            tenant.Details.Alias,
            environment.Humanize());

        EntityListFilters filters = new EntityListFilters
        {
            PageSize = 1000,
            Page = 1,
        };

        Stopwatch totalStopWatch = new Stopwatch();
        totalStopWatch.Start();
        int totalAddedItems = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            IEnumerable<IQuoteSearchIndexWriteModel> quotesToIndex
                = this.quoteReadModelRepository.GetQuotesForSearchIndexCreation(tenant.Id, environment, filters);
            var quotesToIndexCount = quotesToIndex.Count();
            if (quotesToIndexCount == 0)
            {
                break;
            }

            // Since the product name is not part of the policy read model, we need to populate it manually
            PolicyQuoteSearchIndexWriteModelHelper.PopulateProductNames(quotesToIndex, products);

            this.luceneQuoteRepository.AddItemsToRegenerationIndex(tenant, environment, quotesToIndex);
            stopWatch.Stop();
            TimeSpan timeSpan = stopWatch.Elapsed;
            this.logger.LogInformation(
                "Quote index regeneration for tenant {0} in environment {1}: Processed batch {2} ({3} quotes) in {4}.",
                tenant.Details.Alias,
                environment.Humanize(),
                filters.Page,
                quotesToIndexCount,
                timeSpan.Humanize());

            filters.Page++;
            totalAddedItems += quotesToIndexCount;
            if (quotesToIndexCount < filters.PageSize)
            {
                break;
            }
        }

        totalStopWatch.Stop();
        TimeSpan totalTimeSpan = totalStopWatch.Elapsed;
        this.logger.LogInformation(
            "Quote index regeneration for tenant {0} in environment {1}: Completed adding a total {2} quotes to the regeneration index.",
            tenant.Details.Alias,
            environment.Humanize(),
            totalAddedItems,
            totalTimeSpan.Humanize());

        return totalAddedItems;
    }

    public override void MakeRegenerationIndexTheLiveIndex(Tenant tenant, DeploymentEnvironment environment)
    {
        this.luceneQuoteRepository.MakeRegenerationIndexTheLiveIndex(tenant, environment);
        this.logger.LogInformation($"Quote regeneration index has now become the live index for tenant {1} in {0} environment",
            tenant.Details.Alias,
            environment.Humanize());
    }

    public override void MakeSureRegenerationFolderIsEmptyBeforeRegeneration(Tenant tenant, DeploymentEnvironment environment)
    {
        this.luceneQuoteRepository.MakeSureRegenerationFolderIsEmptyBeforeRegeneration(tenant, environment);
    }

    protected override string GetIndexName()
    {
        return "Quote";
    }

    private int AddNewQuotesToSearchIndexSince(
        Tenant tenant,
        DeploymentEnvironment environment,
        IEnumerable<Product> products,
        CancellationToken cancellationToken,
        long? indexLastUpdatedTicksSinceEpoch = null)
    {
        EntityListFilters filters = new EntityListFilters
        {
            PageSize = 1000,
            Page = 1,
        };

        int totalAddedItems = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IEnumerable<IQuoteSearchIndexWriteModel> quotesToIndex
                = this.quoteReadModelRepository.GetQuotesForSearchIndexCreation(
                    tenant.Id, environment, filters, indexLastUpdatedTicksSinceEpoch);
            var quotesToIndexCount = quotesToIndex.Count();
            if (quotesToIndexCount == 0)
            {
                break;
            }

            // Since the product name is not part of the policy read model, we need to populate it manually
            PolicyQuoteSearchIndexWriteModelHelper.PopulateProductNames(quotesToIndex, products);

            this.luceneQuoteRepository.AddItemsToIndex(tenant, environment, quotesToIndex);
            filters.Page++;
            totalAddedItems += quotesToIndexCount;
            if (quotesToIndexCount < filters.PageSize)
            {
                break;
            }
        }

        return totalAddedItems;
    }
}
