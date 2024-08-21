// <copyright file="SpreadsheetPoolService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.FlexCel;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Humanizer.Bytes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NodaTime;
using UBind.Application.Releases;
using UBind.Application.ResourcePool;
using UBind.Application.Services.Email;
using UBind.Domain;
using UBind.Domain.Product;

/// <summary>
/// Reperesents a service to access pools of spreadsheet instances for use in calculating things.
/// </summary>
public class SpreadsheetPoolService : ISpreadsheetPoolService
{
    private readonly ISpreadsheetPoolConfiguration? spreadsheetPoolConfiguration;
    private readonly IErrorNotificationService errorNotificationService;
    private ConcurrentDictionary<Tuple<ReleaseContext, WebFormAppType>, FlexCelWorkbookPool> spreadsheetPoolMap;
    private IClock clock;
    private ILogger<IResourcePool> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpreadsheetPoolService"/> class.
    /// </summary>
    /// <param name="spreadsheetPoolConfiguration">The configuration.</param>
    /// <param name="globalReleaseCache">Global cache of active releases.</param>
    /// <param name="clock">The clock.</param>
    /// <param name="logger">THe logger.</param>
    public SpreadsheetPoolService(
        ISpreadsheetPoolConfiguration? spreadsheetPoolConfiguration,
        IGlobalReleaseCache globalReleaseCache,
        IClock clock,
        ILogger<IResourcePool> logger,
        IErrorNotificationService errorNotificationService)
    {
        this.spreadsheetPoolConfiguration = spreadsheetPoolConfiguration;
        this.clock = clock;
        this.logger = logger;
        this.errorNotificationService = errorNotificationService;
        int numberOfProcessors = Environment.ProcessorCount;
        int numberOfCores = numberOfProcessors * 2;
        int initialCapacity = 200;
        this.spreadsheetPoolMap = new ConcurrentDictionary<Tuple<ReleaseContext, WebFormAppType>, FlexCelWorkbookPool>(
            numberOfCores,
            initialCapacity);

        globalReleaseCache.ReleaseCached += this.OnReleaseCached;
    }

    /// <inheritdoc/>
    public FlexCelWorkbookPool? GetSpreadsheetPool(ReleaseContext releaseContext, WebFormAppType webFormAppType)
    {
        bool succeeded = this.spreadsheetPoolMap.TryGetValue(
            new Tuple<ReleaseContext, WebFormAppType>(releaseContext, webFormAppType),
            out FlexCelWorkbookPool? pool);
        return succeeded ? pool : null;
    }

    /// <inheritdoc/>
    public void CreateSpreadsheetPool(
        ReleaseContext releaseContext,
        WebFormAppType webFormAppType,
        byte[] spreadsheetBytes,
        Guid releaseId,
        IFlexCelWorkbook? initialWorkbook = null,
        bool applicationStartup = false)
    {
        FlexCelWorkbookPool spreadsheetPool = initialWorkbook == null
            ? FlexCelWorkbookPool.CreateEmptyPool(
                releaseContext, webFormAppType, spreadsheetBytes, releaseId, this.clock, this.logger, this.errorNotificationService)
            : FlexCelWorkbookPool.CreatePoolWithSeed(
                releaseContext, webFormAppType, spreadsheetBytes, releaseId, initialWorkbook, this.clock, this.logger, this.errorNotificationService);
        ResourcePoolSizeManager poolSizeManager = new ResourcePoolSizeManager(
            this.clock,
            this.logger,
            this.GetPoolSizeManagerOptions(releaseContext.Environment, applicationStartup, initialWorkbook != null));
        var statsCollector = new ResourcePoolStatsCollector(spreadsheetPool, this.clock);
        spreadsheetPool.StatsCollector = statsCollector;
        poolSizeManager.Manage(spreadsheetPool);
        this.spreadsheetPoolMap.AddOrUpdate(
            new Tuple<ReleaseContext, WebFormAppType>(releaseContext, webFormAppType),
            spreadsheetPool,
            (key, existingValue) =>
            {
                existingValue?.Dispose();
                return spreadsheetPool;
            });
    }

    /// <inheritdoc/>
    public void RemoveSpreadsheetPool(ReleaseContext releaseContext, WebFormAppType webFormAppType)
    {
        this.spreadsheetPoolMap.TryRemove(
            new Tuple<ReleaseContext, WebFormAppType>(releaseContext, webFormAppType),
            out FlexCelWorkbookPool? pool);
        if (pool != null)
        {
            pool.Dispose();
            pool = null;
        }
    }

    /// <inheritdoc/>
    public void RemoveSpreadsheetPools(ProductContext productContext, WebFormAppType webFormAppType)
    {
        var poolKeys = this.spreadsheetPoolMap.Keys.ToList()
            .Where(k => k.Item2 == webFormAppType
                && k.Item1.TenantId == productContext.TenantId
                && k.Item1.ProductId == productContext.ProductId
                && k.Item1.Environment == productContext.Environment);
        foreach (var key in poolKeys)
        {
            this.RemoveSpreadsheetPool(key.Item1, webFormAppType);
        }
    }

    /// <inheritdoc/>
    public JObject GetMemoryUsageInformation()
    {
        JObject result = new JObject();
        JArray pools = new JArray();
        int totalMemoryUsedBytes = 0;
        int totalNumberOfSpreadsheets = 0;

        // adding logging to debug UB-5407
        if (this.spreadsheetPoolMap == null)
        {
            this.logger.LogError("GetMemoryUsageInformation found that spreadsheetPoolMap was null, which should not be possible.");
        }
        else
        {
            foreach (KeyValuePair<Tuple<ReleaseContext, WebFormAppType>, FlexCelWorkbookPool> entry in this.spreadsheetPoolMap)
            {
                // adding logging to debug UB-5407
                if (entry.Value == null)
                {
                    this.logger.LogError(
                        "GetMemoryUsageInformation found that when iterating spreadsheetPoolMap, an entry was found "
                        + "to have a null value, which is unexpected. The entry value is expected to be a non null "
                        + "instance of FlexCelWorkbookPool");
                    continue;
                }

                var poolBytes = entry.Value.GetMemoryUsedBytes();
                var numberOfSpreadsheets = entry.Value.GetResourceCount();
                totalMemoryUsedBytes += poolBytes;
                totalNumberOfSpreadsheets += numberOfSpreadsheets;
                float poolPercentageInUse = (float)entry.Value.GetUsageCount() / (float)numberOfSpreadsheets;

                pools.Add(new JObject()
                {
                    { "tenant", entry.Key.Item1.TenantId },
                    { "product", entry.Key.Item1.ProductId },
                    { "environment", entry.Key.Item1.Environment.ToString() },
                    ////{ "releaseId", entry.Key.Item1 },
                    { "appType", entry.Key.Item2.ToString() },
                    { "numberOfSpreadsheets", numberOfSpreadsheets },
                    { "percentageInUse", poolPercentageInUse },
                    { "poolGrowthThresholdPercent", entry.Value.PoolSizeManager?.PoolGrowthThresholdPercent },
                    { "poolGrowthRatePercent", entry.Value.PoolSizeManager?.PoolGrowthRatePercent },
                    { "memoryUsedBytes", poolBytes },
                    { "memoryUsedFriendly", ByteSize.FromBytes(poolBytes).ToString("#.##") },
                    { "statsLastHour", this.GetStatsLastDuration(entry.Value, Duration.FromHours(1)) },
                    { "statsLastDay", this.GetStatsLastDuration(entry.Value, Duration.FromDays(1)) },
                    { "statsLastWeek", this.GetStatsLastDuration(entry.Value, Duration.FromDays(7)) },
                });
            }
        }

        result.Add(new JProperty("spreadsheetPools", pools));
        result.Add(new JProperty("totals", new JObject()
        {
            { "numberOfPools", pools.Count },
            { "numberOfSpreadsheets", totalNumberOfSpreadsheets },
            { "memoryUsedBytes", totalMemoryUsedBytes },
            { "memoryUsedBytesFriendly", ByteSize.FromBytes(totalMemoryUsedBytes).ToString("#.##") },
        }));
        if (this.spreadsheetPoolConfiguration != null)
        {
            result.Add(new JProperty("configuration", JObject.FromObject(this.spreadsheetPoolConfiguration)));
        }

        return result;
    }

    private JObject GetStatsLastDuration(FlexCelWorkbookPool pool, Duration duration)
    {
        return new JObject()
        {
            {
                "resourceAquired",
                pool.StatsCollector?.GetNumberOfEventsOfTypeInLastDuration(
                    ResourcePoolEvent.EventType.ResourceAquired,
                    duration)
            },
            {
                "resourceReleased",
                pool.StatsCollector?.GetNumberOfEventsOfTypeInLastDuration(
                    ResourcePoolEvent.EventType.ResourceReleased,
                    duration)
            },
            {
                "poolExhausted",
                pool.StatsCollector?.GetNumberOfEventsOfTypeInLastDuration(
                    ResourcePoolEvent.EventType.PoolExhausted,
                    duration)
            },
            {
                "resourceAdded",
                pool.StatsCollector?.GetNumberOfEventsOfTypeInLastDuration(
                    ResourcePoolEvent.EventType.ResourceAdded,
                    duration)
            },
            {
                "resourceRemoved",
                pool.StatsCollector?.GetNumberOfEventsOfTypeInLastDuration(
                    ResourcePoolEvent.EventType.ResourceRemoved,
                    duration)
            },
            {
                "poolGrown",
                pool.StatsCollector?.GetNumberOfEventsOfTypeInLastDuration(
                    ResourcePoolEvent.EventType.PoolGrown,
                    duration)
            },
            {
                "resourceWasted",
                pool.StatsCollector?.GetNumberOfEventsOfTypeInLastDuration(
                    ResourcePoolEvent.EventType.ResourceWasted,
                    duration)
            },
        };
    }

    private ResourcePoolSizeManager.Options GetPoolSizeManagerOptions(DeploymentEnvironment environment, bool applicationStartup, bool hasSeedWorkbookFromSync)
    {
        if (this.spreadsheetPoolConfiguration == null)
        {
            // use the hard coded defaults
            return new ResourcePoolSizeManager.Options();
        }

        int minimumPoolSize;
        switch (environment)
        {
            case DeploymentEnvironment.Development:
                minimumPoolSize = this.spreadsheetPoolConfiguration.MinimumPoolSizeDevelopment;
                break;
            case DeploymentEnvironment.Staging:
                minimumPoolSize = this.spreadsheetPoolConfiguration.MinimumPoolSizeStaging;
                break;
            case DeploymentEnvironment.Production:
                minimumPoolSize = this.spreadsheetPoolConfiguration.MinimumPoolSizeProduction;
                break;
            default:
                throw new ArgumentException(
                    "Came across a deployment environment other than development, staging or production.");
        }

        return new ResourcePoolSizeManager.Options()
        {
            MinimumResourceLifeDuration
                = Duration.FromMinutes(this.spreadsheetPoolConfiguration.MinimumResourceLifeMinutes),
            ResourceUnusedStaleDuration
                = Duration.FromMinutes(this.spreadsheetPoolConfiguration.ResourceUnusedStaleMinutes),
            PoolReaperInterval
                = Duration.FromMinutes(this.spreadsheetPoolConfiguration.PoolReaperIntervalMinutes),
            MinimumPoolSize = minimumPoolSize,
            PoolExhaustedGrowthThresholdPercentReduction
                = this.spreadsheetPoolConfiguration.PoolExhaustedGrowthThresholdPercentReduction,
            ResourcesWastedGrowthThresholdPercentIncrease
                = this.spreadsheetPoolConfiguration.ResourcesWastedGrowthThresholdPercentIncrease,
            PoolExhaustedGrowthRatePercentIncrease
                = this.spreadsheetPoolConfiguration.ResourcesWastedGrowthRatePercentReduction,
            InitialPoolGrowthThresholdPercent
                = this.spreadsheetPoolConfiguration.InitialPoolGrowthThresholdPercent,
            InitialPoolGrowthRatePercent
                = this.spreadsheetPoolConfiguration.InitialPoolGrowthRatePercent,
            CreationGrowthDelayDuration = applicationStartup
                ? Duration.FromMilliseconds(this.spreadsheetPoolConfiguration.StartupGrowthDelayMilliseconds)
                : hasSeedWorkbookFromSync
                    ? Duration.FromMilliseconds(this.spreadsheetPoolConfiguration.SyncCreationGrowthDelayMilliseconds)
                    : Duration.FromMilliseconds(this.spreadsheetPoolConfiguration.CreationGrowthDelayMilliseconds),
            CreationGrowthIntervalDuration = applicationStartup
                ? Duration.FromMilliseconds(this.spreadsheetPoolConfiguration.StartupGrowthIntervalMilliseconds)
                : Duration.FromMilliseconds(this.spreadsheetPoolConfiguration.CreationGrowthIntervalMilliseconds),
            CreationGrowthDelayRandomMaxDuration = applicationStartup
                ? Duration.FromMilliseconds(this.spreadsheetPoolConfiguration.AdditionalStartupGrowthDelayRandomMaxMilliseconds)
                : Duration.Zero,
        };
    }

    private void OnReleaseCached(object? sender, ReleaseCachingArgs e)
    {
        if (e.Release[WebFormAppType.Quote] != null)
        {
            this.CreatePoolOnCache(WebFormAppType.Quote, e);
        }

        if (e.Release[WebFormAppType.Claim] != null)
        {
            this.CreatePoolOnCache(WebFormAppType.Claim, e);
        }
    }

    private void CreatePoolOnCache(WebFormAppType formType, ReleaseCachingArgs args)
    {
        var existingPool = this.GetSpreadsheetPool(args.Release.ReleaseContext, formType);
        if (existingPool == null || existingPool.ReleaseId != args.Release.ReleaseId)
        {
            this.CreateSpreadsheetPool(
                    args.Release.ReleaseContext,
                    formType,
                    args.Release[formType].WorkbookData,
                    args.Release.ReleaseId,
                    formType == WebFormAppType.Quote ? args.QuoteWorkbook : args.ClaimWorkbook);
        }
    }
}
