// <copyright file="PolicyLuceneIndexService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Search;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

public class PolicyLuceneIndexService : BaseLuceneIndexService, ISearchableEntityService<IPolicySearchResultItemReadModel, PolicyReadModelFilters>
{
    private readonly IPolicyReadModelRepository policyReadModelRepository;
    private readonly ILuceneRepository<IPolicySearchIndexWriteModel, IPolicySearchResultItemReadModel, PolicyReadModelFilters> lucenePolicyRepository;
    private readonly ILogger<PolicyLuceneIndexService> logger;
    private readonly IErrorNotificationService errorNotificationService;

    public PolicyLuceneIndexService(
        IPolicyReadModelRepository policyReadModelRepository,
        ITenantRepository tenantRepository,
        ILuceneRepository<IPolicySearchIndexWriteModel, IPolicySearchResultItemReadModel, PolicyReadModelFilters> lucenePolicyRepository,
        ILogger<PolicyLuceneIndexService> logger,
        IErrorNotificationService errorNotificationService)
        : base(tenantRepository, logger)
    {
        this.policyReadModelRepository = policyReadModelRepository;
        this.lucenePolicyRepository = lucenePolicyRepository;
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
        Instant fromTimestamp,
        Instant toTimestamp)
    {
        var repositoryCount = this.policyReadModelRepository.GetPolicyCountBetweenDates(tenant.Id, environment, fromTimestamp, toTimestamp);
        var luceneIndexCount = this.lucenePolicyRepository.GetEntityIndexCountBetweenDates(tenant, environment, fromTimestamp, toTimestamp);
        return new EntityRepositoryAndLuceneIndexCountModel(tenant.Details?.Alias, repositoryCount, luceneIndexCount);
    }

    /// <inheritdoc/>
    public IEnumerable<IPolicySearchResultItemReadModel> Search(
        Tenant tenant,
        DeploymentEnvironment environment,
        PolicyReadModelFilters filters)
    {
        try
        {
            return this.lucenePolicyRepository.Search(tenant, environment, filters);
        }
        catch (CorruptIndexException ex)
        {
            var exception = new ErrorException(
                Errors.LuceneIndex.SearchIndexCorrupted(tenant.Details.Alias, environment, LuceneIndexType.Policy.Humanize().ToLower(), ex.Message));
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
                    "Starting policy index regeneration in {0} environment for all tenants.",
                    environment.Humanize());
            }
            else
            {
                string tenantAliases = string.Join(", ", tenants.Select(t => t.Details.Alias));
                this.logger.LogInformation(
                    "Starting policy index regeneration in {0} environment for tenants: {1}.",
                    environment.Humanize(),
                    tenantAliases);
            }
            this.RegenerateEntityLuceneIndexes(environment, products, cancellationToken, tenants);
            this.logger.LogInformation(
                "Completed policy index regeneration in {0} environment.",
                environment.Humanize());
        }
        catch (Exception exception)
        {
            this.logger.LogError(
                "Failed regenerating policies index in {0} environment: {1}",
                environment,
                exception.Message);
            this.errorNotificationService.CaptureSentryException(exception, environment);
            throw;
        }
    }

    public override int ProcessLuceneIndexUpdates(
        Tenant tenant,
        DeploymentEnvironment environment,
        IEnumerable<Product> products,
        CancellationToken cancellationToken)
    {
        try
        {
            var indexLastUpdatedTicksSinceEpoch
                = this.lucenePolicyRepository.GetIndexLastUpdatedTicksSinceEpoch(tenant, environment);

            return this.AddNewPoliciesToSearchIndexSince(
                tenant,
                environment,
                products,
                cancellationToken,
                indexLastUpdatedTicksSinceEpoch);
        }
        catch (Exception exception)
        {
            this.logger.LogError(
                "Failed updating the policies index for tenant {0} in environment {1}: {2}",
                tenant.Details.Alias,
                environment.Humanize(),
                exception.Message);

            var additionalDetails = this.errorNotificationService.GetErrorDetails(
                tenant.Details.Alias,
                environment.Humanize(),
                exception,
                $"The exception originated from: {LuceneIndexType.Policy.Humanize()}");
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
            "Policy index regeneration for tenant {0} in environment {1}: Adding policies to the regeneration index.",
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
            IEnumerable<IPolicySearchIndexWriteModel> policiesToIndex
                = this.policyReadModelRepository.GetPolicyForSearchIndexCreation(tenant.Id, environment, filters);
            var policiesToIndexCount = policiesToIndex.Count();

            // Sometimes, due to the group join, the number of policies returned is less than the page size,
            // even though there are more policies to index. This is why we can only stop once we get 0 policies in this batch.
            if (policiesToIndexCount == 0)
            {
                break;
            }

            // Since the product name is not part of the policy read model, we need to populate it manually
            PolicyQuoteSearchIndexWriteModelHelper.PopulateProductNames(policiesToIndex, products);

            this.lucenePolicyRepository.AddItemsToRegenerationIndex(tenant, environment, policiesToIndex);
            stopWatch.Stop();
            TimeSpan timeSpan = stopWatch.Elapsed;
            this.logger.LogInformation(
                "Policy index regeneration for tenant {0} in environment {1}: Processed batch {2} ({3} policies) in {4}.",
                tenant.Details.Alias,
                environment.Humanize(),
                filters.Page,
                policiesToIndexCount,
                timeSpan.Humanize());

            filters.Page++;
            totalAddedItems += policiesToIndexCount;
        }

        totalStopWatch.Stop();
        TimeSpan totalTimeSpan = totalStopWatch.Elapsed;
        this.logger.LogInformation(
            "Policy index regeneration for tenant {0} in environment {1}: Completed adding a total {2} policies to the regeneration index in {3}",
            tenant.Details.Alias,
            environment.Humanize(),
            totalAddedItems,
            totalTimeSpan.Humanize());

        return totalAddedItems;
    }

    public override void MakeRegenerationIndexTheLiveIndex(Tenant tenant, DeploymentEnvironment environment)
    {
        this.lucenePolicyRepository.MakeRegenerationIndexTheLiveIndex(tenant, environment);
        this.logger.LogInformation($"Policy regeneration index has now become the live index for tenant {1} in {0} environment",
            tenant.Details.Alias,
            environment.Humanize());
    }

    public override void MakeSureRegenerationFolderIsEmptyBeforeRegeneration(Tenant tenant, DeploymentEnvironment environment)
    {
        this.lucenePolicyRepository.MakeSureRegenerationFolderIsEmptyBeforeRegeneration(tenant, environment);
    }

    protected override string GetIndexName()
    {
        return "Policy";
    }

    private int AddNewPoliciesToSearchIndexSince(
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
            IEnumerable<IPolicySearchIndexWriteModel> policiesToIndex
                = this.policyReadModelRepository.GetPolicyForSearchIndexCreation(
                    tenant.Id, environment, filters, indexLastUpdatedTicksSinceEpoch);
            var policiesToIndexCount = policiesToIndex.Count();

            // Sometimes, due to the group join, the number of policies returned is less than the page size,
            // even though there are more policies to index. This is why we can only stop once we get 0 policies in this batch.
            if (policiesToIndexCount == 0)
            {
                break;
            }

            // Since the product name is not part of the policy read model, we need to populate it manually
            PolicyQuoteSearchIndexWriteModelHelper.PopulateProductNames(policiesToIndex, products);

            this.lucenePolicyRepository.AddItemsToIndex(tenant, environment, policiesToIndex);
            filters.Page++;
            totalAddedItems += policiesToIndexCount;
            cancellationToken.ThrowIfCancellationRequested();
        }

        return totalAddedItems;
    }
}
