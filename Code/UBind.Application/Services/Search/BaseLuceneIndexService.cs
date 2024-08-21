// <copyright file="BaseLuceneIndexService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Search;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Humanizer;
using Microsoft.Extensions.Logging;
using UBind.Domain;
using UBind.Domain.Product;
using UBind.Domain.Repositories;

public abstract class BaseLuceneIndexService
{
    private readonly ITenantRepository tenantRepository;
    private readonly ILogger<BaseLuceneIndexService> logger;

    public BaseLuceneIndexService(
        ITenantRepository tenantRepository,
        ILogger<BaseLuceneIndexService> logger)
    {
        this.tenantRepository = tenantRepository;
        this.logger = logger;
    }

    public abstract int ProcessLuceneIndexUpdates(
        Tenant tenant,
        DeploymentEnvironment environment,
        IEnumerable<Product> products,
        CancellationToken cancellationToken);

    public abstract int AddEntitiesToLuceneRegenerationIndex(
        Tenant tenant,
        DeploymentEnvironment environment,
        IEnumerable<Product> products,
        CancellationToken cancellationToken);

    public abstract void MakeSureRegenerationFolderIsEmptyBeforeRegeneration(Tenant tenant, DeploymentEnvironment environment);

    public abstract void MakeRegenerationIndexTheLiveIndex(Tenant tenant, DeploymentEnvironment environment);

    protected void ProcessLuceneIndexUpdatesForAllTenants(
        DeploymentEnvironment environment,
        IEnumerable<Product> products,
        CancellationToken cancellationToken)
    {
        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();

        int totalItemsAdded = 0;
        var tenants = this.GetTenants();
        foreach (var tenant in tenants)
        {
            totalItemsAdded += this.ProcessLuceneIndexUpdates(tenant, environment, products, cancellationToken);
        }

        stopWatch.Stop();
        TimeSpan ts = stopWatch.Elapsed;

        this.logger.LogInformation(string.Format(
            "{0} index updates in {1} environment: Completed adding a total of {2} items to the index in {3}",
            this.GetIndexName(),
            environment.Humanize(),
            totalItemsAdded,
            ts.Humanize()));
    }

    protected abstract string GetIndexName();

    protected void RegenerateEntityLuceneIndexes(
        DeploymentEnvironment environment,
        IEnumerable<Product> products,
        CancellationToken cancellationToken,
        IEnumerable<Tenant> tenants = null)
    {
        tenants ??= this.GetTenants();
        foreach (var tenant in tenants)
        {
            this.MakeSureRegenerationFolderIsEmptyBeforeRegeneration(tenant, environment);

            var totalIndexes = this.AddEntitiesToLuceneRegenerationIndex(tenant, environment, products, cancellationToken);
            if (totalIndexes > 0)
            {
                this.MakeRegenerationIndexTheLiveIndex(tenant, environment);
            }

            // Reprocess Lucene Index for new entity lucene added whilst the index is being regenerated.
            this.ReprocessEntitiesCreatedDuringLuceneIndexRegeneration(tenant, environment, products, cancellationToken);
        }
    }

    protected void ReprocessEntitiesCreatedDuringLuceneIndexRegeneration(
        Tenant tenant,
        DeploymentEnvironment environment,
        IEnumerable<Product> products,
        CancellationToken cancellationToken)
    {
        this.ProcessLuceneIndexUpdates(tenant, environment, products, cancellationToken);
    }

    private List<Tenant> GetTenants(IEnumerable<Guid> tenantIds = null)
    {
        List<Tenant> tenants = new List<Tenant>();
        if (tenantIds != null)
        {
            foreach (var tenantId in tenantIds)
            {
                var tenant = this.tenantRepository.GetTenantById(tenantId);
                tenants.Add(tenant);
            }
        }
        else
        {
            tenants = this.tenantRepository.GetActiveTenants()
                .Where(x => x.Id != Tenant.MasterTenantId).ToList();
        }

        return tenants;
    }
}
