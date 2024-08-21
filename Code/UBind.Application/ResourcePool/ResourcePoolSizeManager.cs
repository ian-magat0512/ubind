// <copyright file="ResourcePoolSizeManager.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.ResourcePool
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Application.Automation.Extensions;
    using UBind.Domain.Exceptions;
    using Timer = System.Timers.Timer;

    /// <summary>
    /// An algorithm for managing a pool of resources, so that it grows and shrinks to meet demand.
    /// 1. Upon application startup, a pool of 10 (MinimumPoolSize) resources are created. Upon creation of a resource,
    ///    it's created timestamp (createdTimestamp) is recorded.
    /// 2. When a request comes in to use a resource, the resource is taken out of the available pool.
    /// 3. When once the resource has been used, it's returned to the available pool.
    /// 4. Pool growth: If the number of resources used (unavailable) is greater than or
    ///    equal to the 60% mark (poolGrowthThresholdPercent) of the total pool size, a number of pool resources will
    ///    be created - an amount of 50% (poolGrowthRatePercent) of the total number of pool resources. This will ensure
    ///    the pool can handle a spike in requests.E.g. if there are 10 resources, and 6 are in use,
    ///    it will create 5 more.
    /// 6. The creation of new resources will happen asynchronously so as not to add additional time to any
    ///    one request, and upon creation of a new resource, it's created timestamp is set.
    /// 7. Pool shrinkage: An independent resource reaper will periodically, every X minutes
    ///    (poolInspectionFrequency) inspect all resources in the pool which are in are not currently used
    ///    and delete them if they have both:
    ///       a) been in existance for longer than 10 minutes(MinimumResourceLifeDuration) (being the time since the
    ///          createdTimestamp); and
    ///       b) not been used in the last 10 minutes(ResourceUnusedStaleDuration) (being the time since the last used
    ///          timestamp).
    /// 8. If a request comes in and there are no free resources, then a new resource is created
    ///    synchronously, added to the pool and used by the request. Additionally, the poolGrowthThresholdPercent is
    ///    reduced by 5% and the poolGrowthRatePercent is increased by 5%.
    /// 9. If during pool shrinkage, the reaper thread finds a resource that was never used (ie it's creation
    ///    timestamp is more than 10 minutes ago and it's last used timestamp is null) then the
    ///    poolGrowthThresholdPercent is increased by 5% and the poolGrowthRatePercent is reduced by 5%.
    /// </summary>
    public class ResourcePoolSizeManager : IDisposable
    {
        /// <summary>
        /// An object to lock on to stop two threads from running grow/shrink at the same time.
        /// </summary>
        private readonly object growShrinkLock = new object();

        /// <summary>
        /// An object to lock on to stop multiple threads from starting/stopping a singular timer at the same time.
        /// </summary>
        private readonly object timerAccessLock = new object();

        private IResourcePool? pool;
        private IClock clock;
        private ILogger logger;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Timer? timer;
        private bool startupComplete;
        private Duration startupGrowthDelayDuration;
        private string? poolDebugIdentifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourcePoolSizeManager"/> class.
        /// </summary>
        /// <param name="pool">The pool to manage.</param>
        /// <param name="clock">The clock.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="options">The operational settings, if the defaults are not acceptable.</param>
        public ResourcePoolSizeManager(
            IClock clock,
            ILogger logger,
            Options? options = null)
        {
            this.clock = clock;
            this.logger = logger;
            this.Settings = options ?? new Options();
            this.TargetPoolSize = this.Settings.MinimumPoolSize;
            this.PoolGrowthThresholdPercent = this.Settings.InitialPoolGrowthThresholdPercent;
            this.PoolGrowthRatePercent = this.Settings.InitialPoolGrowthRatePercent;
            this.startupGrowthDelayDuration = this.Settings.CreationGrowthDelayDuration;
            if (this.Settings.CreationGrowthDelayRandomMaxDuration > Duration.Zero)
            {
                var rand = new Random();
                int additionalGrowthDelayRandomMillis = rand.Next(
                    (int)this.Settings.CreationGrowthDelayRandomMaxDuration.TotalMilliseconds);
                this.startupGrowthDelayDuration += Duration.FromMilliseconds(additionalGrowthDelayRandomMillis);
            }
        }

        /// <summary>
        /// An event which is published when the pool size has grown to the minimum size, which is what happens on
        /// startup.
        /// </summary>
        public event EventHandler? StartupCompleted;

        /// <summary>
        /// An event which is published each time GrowPoolToTargetSize completes executing.
        /// </summary>
        public event EventHandler? GrowPoolCompleted;

        /// <summary>
        /// An event which is published each time a ReapUnused cycle starts executing.
        /// </summary>
        public event EventHandler? ReapUnusedCycleStarted;

        /// <summary>
        /// An event which is published each time a ReapUnused cycle completes executing.
        /// </summary>
        public event EventHandler? ReapUnusedCycleCompleted;

        /// <summary>
        /// An event which is published each time a Resource was found to have been wasted.
        /// A wasted resource is one that was created, but was then deleted without being used.
        /// </summary>
        public event EventHandler? ResourcesWasted;

        /// <summary>
        /// An event which is published each time a ReapUnused cycle completes executing.
        /// </summary>
        public event EventHandler? PoolExhausted;

        /// <summary>
        /// Gets the operational settings.
        /// </summary>
        public Options Settings { get; private set; }

        /// <summary>
        /// Gets the target number of intances that should be kept in the pool.
        /// </summary>
        public int TargetPoolSize { get; private set; }

        /// <summary>
        /// Gets a value indicating after what perecent of usage the pool should be grown.
        /// </summary>
        public float PoolGrowthThresholdPercent { get; private set; }

        /// <summary>
        /// Gets the percentage to grow the pool by when it's usage exceeds the threshold.
        /// </summary>
        public float PoolGrowthRatePercent { get; private set; }

        /* TODO: Remove this, it's just for temporary debugging. Will be removed in ticket UB-10151 */
        public string? StartupLog { get; set; }

        /// <summary>
        /// Starts the process of managing the given pool.
        /// </summary>
        public void Manage(IResourcePool pool)
        {
            this.StartupLog += "Manage1\n";
            if (this.pool != null)
            {
                throw new InvalidOperationException("You cannot use a ResourcePoolSizeManager to manage another pool. "
                    + "Please create a separate instance of ResourcePoolSizeManager for each pool to be managed.");
            }

            this.pool = pool;
            this.poolDebugIdentifier = pool.GetDebugIdentifier();
            this.pool.PoolSizeManager = this;
            this.StartupCompleted += this.RunReaperOnStartupCompletion;
            this.StartupLog += "Manage2\n";
            Task.Run(this.GrowPoolToMinimumSize, this.cancellationTokenSource.Token)
                .ContinueWith(
                    t =>
                    {
                        if (t.IsFaulted)
                        {
                            if (t.Exception != null)
                            {
                                foreach (var ex in t.Exception.InnerExceptions)
                                {
                                    if (ex is ErrorException eex)
                                    {
                                        eex.EnrichAndRethrow();
                                    }
                                }
                            }
                        }
                    },
                    TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// Called when the pool has been asked for an instance but it has no free instances.
        /// </summary>
        public void OnPoolExhausted()
        {
            lock (this.growShrinkLock)
            {
                if (this.startupComplete && this.pool != null)
                {
                    this.PoolGrowthThresholdPercent -= this.Settings.PoolExhaustedGrowthThresholdPercentReduction;
                    this.PoolGrowthRatePercent += this.Settings.PoolExhaustedGrowthRatePercentIncrease;
                    this.logger.LogInformation(
                        "ResourcePoolSizeManager: {DebugIdentifier} - Pool was just exhausted with {ResourceCount} all being used. "
                        + "Reducing PoolGrowthThresholdPercent to {PoolGrowthThresholdPercent} and "
                        + "increasing PoolGrowthRatePercent to {PoolGrowthRatePercent}.",
                        this.poolDebugIdentifier,
                        this.pool.GetResourceCount(),
                        this.PoolGrowthThresholdPercent,
                        this.PoolGrowthRatePercent);
                    this.PoolExhausted?.Invoke(this, new EventArgs());
                    this.UpdateTargetPoolSize();
                    Task.Run(this.GrowPoolToTargetSize, this.cancellationTokenSource.Token);
                }
            }
        }

        /// <summary>
        /// Called when a resource has been consumed from the pool.
        /// </summary>
        public void OnResourceConsumed()
        {
            if (this.startupComplete)
            {
                lock (this.growShrinkLock)
                {
                    if (this.pool != null)
                    {
                        this.UpdateTargetPoolSize();
                        if (this.pool != null && this.pool.GetResourceCount() < this.TargetPoolSize)
                        {
                            Task.Run(this.GrowPoolToTargetSize, this.cancellationTokenSource.Token);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Called when the reaper thread finds more than one workbook that was never used within the alloted time period (MinimumWorkbookLifeMinutes).
        /// </summary>
        public void OnResourcesWasted()
        {
            this.PoolGrowthThresholdPercent += this.Settings.ResourcesWastedGrowthThresholdPercentIncrease;
            this.PoolGrowthRatePercent -= this.Settings.ResourcesWastedGrowthRatePercentReduction;
            this.logger.LogInformation(
                "ResourcePoolSizeManager: {DebugIdentifier} - Because more than one resource was wasted, "
                + "increasing PoolGrowthThresholdPercent to {PoolGrowthThresholdPercent} and "
                + "reducing PoolGrowthRatePercent to {PoolGrowthRatePercent}.",
                this.poolDebugIdentifier,
                this.PoolGrowthThresholdPercent,
                this.PoolGrowthRatePercent);
        }

        /// <summary>
        /// Called by the pool when it's disposed, so that we can stop managing it.
        /// </summary>
        public void Dispose()
        {
            this.StartupLog += "Dispose\n";
            this.StartupCompleted -= this.RunReaperOnStartupCompletion;
            lock (this.growShrinkLock)
            {
                if (this.pool != null)
                {
                    // if pool wasn't disposed of already
                    this.pool = null;
                }
            }

            lock (this.timerAccessLock)
            {
                if (this.timer != null)
                {
                    this.timer.Stop();
                    this.timer.Close();
                }
            }

            if (this.cancellationTokenSource != null)
            {
                try
                {
                    this.cancellationTokenSource.Cancel();
                    this.cancellationTokenSource.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // it's already been disposed so no need to do anything.
                }
            }
        }

        /// <summary>
        /// Re-evaluates the target pool size based upon current parameters.
        /// </summary>
        private void UpdateTargetPoolSize()
        {
            int previousTargetPoolSize = this.TargetPoolSize;
            int resourceCount = this.pool?.GetResourceCount() ?? 0;
            int usageCount = this.pool?.GetUsageCount() ?? 0;
            float usagePercent = (float)usageCount / (float)resourceCount;
            if (usagePercent >= this.PoolGrowthThresholdPercent)
            {
                this.TargetPoolSize = Math.Max(
                        (int)((float)resourceCount * (1.0f + this.PoolGrowthRatePercent)),
                        this.Settings.MinimumPoolSize);
                if (previousTargetPoolSize != this.TargetPoolSize)
                {
                    this.logger.LogInformation(
                        "ResourcePoolSizeManager: {DebugIdentifier} - TargetPoolSize was just increased from "
                        + "{PreviousTargetPoolSize} to {TargetPoolSize} which is an increase of {PoolGrowthRatePercent}, "
                        + "because the usage percent was {UsagePercent} and the PoolGrowthThresholdPercent was "
                        + "{PoolGrowthThresholdPercent}.",
                        this.poolDebugIdentifier,
                        previousTargetPoolSize,
                        this.TargetPoolSize,
                        this.PoolGrowthRatePercent,
                        usagePercent,
                        this.PoolGrowthThresholdPercent);
                }
            }
        }

        /// <summary>
        /// Grows or shrinks the pool to match the target size.
        /// </summary>
        private void GrowPoolToTargetSize()
        {
            // Lock to ensure only one thread runs this at a time
            lock (this.growShrinkLock)
            {
                if (this.pool != null)
                {
                    int delta = this.TargetPoolSize - this.pool.GetResourceCount();
                    for (int i = 0; i < delta; i++)
                    {
                        this.pool.AddResource();
                    }

                    this.GrowPoolCompleted?.Invoke(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// During startup, grows the pool to the minimum size.
        /// </summary>
        private async Task GrowPoolToMinimumSize()
        {
            this.StartupLog += "GrowPoolToMinimumSize1\n";
            if (this.startupGrowthDelayDuration > Duration.Zero)
            {
                this.logger.LogInformation(
                    "ResourcePoolSizeManager: {DebugIdentifier} - Sleeping for {SleepSeconds} seconds before growing pool to minimim size "
                    + "{MinimumPoolSize}.",
                    this.poolDebugIdentifier,
                    this.startupGrowthDelayDuration.TotalSeconds,
                    this.Settings.MinimumPoolSize);
                this.StartupLog += "GrowPoolToMinimumSize2\n";
                await Task.Delay(this.startupGrowthDelayDuration.ToTimeSpan());
            }

            this.StartupLog += "GrowPoolToMimimumSize3\n";
            int poolResourceCount = -1;
            lock (this.growShrinkLock)
            {
                this.StartupLog += "GrowPoolToMinimumSize4\n";
                poolResourceCount = this.pool == null ? -1 : this.pool.GetResourceCount();
            }

            if (poolResourceCount == -1)
            {
                this.StartupLog += "GrowPoolToMinimumSize5\n";
                return;
            }

            this.StartupLog += "GrowPoolToMinimumSize6\n";
            int delta = this.Settings.MinimumPoolSize - poolResourceCount;
            if (delta > 0)
            {
                this.StartupLog += "GrowPoolToMinimumSize7\n";
                this.logger.LogInformation(
                    "ResourcePoolSizeManager: {DebugIdentifier} - Growing pool to minimim size "
                    + "{MinimumPoolSize}. There are currently {ResourceCount} resources, adding {delta} resources.",
                    this.poolDebugIdentifier,
                    this.Settings.MinimumPoolSize,
                    poolResourceCount,
                    delta);
            }

            this.StartupLog += "GrowPoolToMinimumSize8\n";
            while (poolResourceCount < this.Settings.MinimumPoolSize)
            {
                this.StartupLog += "    While Loop start iteration\n";
                lock (this.growShrinkLock)
                {
                    if (this.pool == null)
                    {
                        this.StartupLog += "    Pool was null, exiting while loop\n";
                        break;
                    }
                }

                if (this.Settings.CreationGrowthIntervalDuration > Duration.Zero)
                {
                    this.StartupLog += $"    Delaying growth {this.Settings.CreationGrowthIntervalDuration.TotalMilliseconds}ms\n";
                    await Task.Delay(this.Settings.CreationGrowthIntervalDuration.ToTimeSpan());
                }
                else
                {
                    this.StartupLog += $"    No growth delay\n";
                }

                this.StartupLog += "    About to get lock for adding a resource\n";
                lock (this.growShrinkLock)
                {
                    if (this.pool == null)
                    {
                        this.StartupLog += "    Pool was null, exiting while loop\n";
                        break;
                    }

                    this.StartupLog += "    Adding a resource\n";
                    this.pool.AddResource();
                    this.StartupLog += "    Finished adding a resource, getting resource count\n";
                    poolResourceCount = this.pool.GetResourceCount();
                    this.StartupLog += $"    Finished getting resource count: {poolResourceCount}\n";
                }
            }

            this.startupComplete = true;
            this.StartupLog += "About to get lock for getting resource count\n";
            lock (this.growShrinkLock)
            {
                this.StartupLog += "About to get resource count\n";
                poolResourceCount = this.pool == null ? -1 : this.pool.GetResourceCount();
            }

            this.StartupLog += $"resource count is {poolResourceCount}\n";
            if (poolResourceCount == -1)
            {
                this.StartupLog += "Startup growth cancelled, pool was disposed of\n";
                this.logger.LogInformation(
                    "ResourcePoolSizeManager: {DebugIdentifier} - Startup growth cancelled, pool was disposed of.",
                    this.poolDebugIdentifier);
            }
            else
            {
                this.StartupLog += "Startup growth complete\n";
                this.logger.LogInformation(
                    "ResourcePoolSizeManager: {DebugIdentifier} - Startup growth complete. There are now {ResourceCount} resources in the pool.",
                    this.poolDebugIdentifier,
                    poolResourceCount);
                this.StartupLog += "Invoking StartupCompleted EventHandler\n";
                this.StartupCompleted?.Invoke(this, new EventArgs());
            }

            this.StartupLog += "End of method\n";
        }

        private void RunReaperOnStartupCompletion(object? sender, EventArgs e)
        {
            lock (this.timerAccessLock)
            {
                this.timer = new Timer(this.Settings.PoolReaperInterval.TotalMilliseconds);
                this.timer.Elapsed += this.ReapUnused;
                this.timer.AutoReset = true;
                this.timer.Enabled = true;
            }
        }

        /// <summary>
        /// Periodically reaps unused workbooks and then adjusts the growth parameters.
        /// </summary>
        private void ReapUnused(object? source, ElapsedEventArgs e)
        {
            if (this.cancellationTokenSource.Token.IsCancellationRequested)
            {
                this.timer?.Close();
                return;
            }

            this.ReapUnusedCycleStarted?.Invoke(this, new EventArgs());

            int numberOfResourcesWasted = 0;
            lock (this.growShrinkLock)
            {
                if (this.pool == null)
                {
                    return;
                }

                if (this.pool.GetResourceCount() > this.Settings.MinimumPoolSize)
                {
                    var now = this.clock.GetCurrentInstant();
                    var availableInstances = this.pool.GetAvailableResources();
                    foreach (var instance in availableInstances)
                    {
                        var age = now - instance.CreatedTimestamp;
                        bool instanceWasNeverUsed = instance.LastUsedTimestamp == default;
                        if (instanceWasNeverUsed
                            && age > this.Settings.MinimumResourceLifeDuration)
                        {
                            numberOfResourcesWasted++;
                            this.pool.RemoveResource();
                            this.logger.LogInformation(
                                "ResourcePoolSizeManager: {DebugIdentifier} - A resource was just removed from the pool "
                                + "because it was never used and it's age was greater than {MinimumResourceLifeDuration}. "
                                + "The pool now has {ResourceCount} resources.",
                                this.poolDebugIdentifier,
                                this.Settings.MinimumResourceLifeDuration.ToString(),
                                this.pool.GetResourceCount());
                        }
                        else if (!instanceWasNeverUsed)
                        {
                            var unusedDuration = now - instance.LastUsedTimestamp;
                            if (age > this.Settings.MinimumResourceLifeDuration
                                && unusedDuration > this.Settings.ResourceUnusedStaleDuration)
                            {
                                this.pool.RemoveResource();
                                this.logger.LogInformation(
                                    "ResourcePoolSizeManager: {DebugIdentifier} - A resource was just removed from the pool "
                                    + "because had not been used for {UnusedDuration} which is greater than the stale "
                                    + "duration of {ResourceUnusedStaleDuration}. "
                                    + "The pool now has {ResourceCount} resources.",
                                    this.poolDebugIdentifier,
                                    unusedDuration.ToString(),
                                    this.Settings.ResourceUnusedStaleDuration.ToString(),
                                    this.pool.GetResourceCount());
                            }
                        }

                        // No point continuing reaping if we have hit the minimum resource count
                        if (this.pool.GetResourceCount() <= this.Settings.MinimumPoolSize)
                        {
                            break;
                        }
                    }
                }
            }

            if (numberOfResourcesWasted > 1)
            {
                this.OnResourcesWasted();
                this.ResourcesWasted?.Invoke(this, new EventArgs());
            }

            this.ReapUnusedCycleCompleted?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Operational settings for the ResourcePoolSizeManager.
        /// </summary>
        public class Options
        {
            /// <summary>
            /// Gets or sets the minimum amount of time a workbook but be in existence before it can be reaped.
            /// </summary>
            public Duration MinimumResourceLifeDuration { get; set; } = Duration.FromMinutes(10);

            /// <summary>
            /// Gets or sets the amount of time an instance should be unused for to be considered stale.
            /// </summary>
            public Duration ResourceUnusedStaleDuration { get; set; } = Duration.FromMinutes(10);

            /// <summary>
            /// Gets or sets the time between when the reaper runs to check if any workbooks should be reaped.
            /// </summary>
            public Duration PoolReaperInterval { get; set; } = Duration.FromMinutes(10);

            /// <summary>
            /// Gets or sets the minimum number of instances that should be maintained in the pool.
            /// </summary>
            public int MinimumPoolSize { get; set; } = 5;

            /// <summary>
            /// Gets or sets the amount to reduce the growth threshold by so that we grow the pool sooner in future.
            /// This is applicable when the pool is exhausted.
            /// </summary>
            public float PoolExhaustedGrowthThresholdPercentReduction { get; set; } = 0.05f;

            /// <summary>
            /// Gets or sets the amount to increase the growth threshold by so that we grow the pool later in the future.
            /// This is applicabl when more than one pool member is reaped without having been used.
            /// </summary>
            public float ResourcesWastedGrowthThresholdPercentIncrease { get; set; } = 0.05f;

            /// <summary>
            /// Gets or sets the amount to increase the growth rate by so that we grow the pool larger when it it's the threshold.
            /// This is applicable when the pool is exhausted.
            /// </summary>
            public float PoolExhaustedGrowthRatePercentIncrease { get; set; } = 0.05f;

            /// <summary>
            /// Gets or sets the amount to reduce the growth rate percent by so that we grow the pool by a smaller amount in the future.
            /// This is applicable when more than one pool member is reaped without having been used.
            /// </summary>
            public float ResourcesWastedGrowthRatePercentReduction { get; set; } = 0.05f;

            /// <summary>
            /// Gets or sets the initial value for the pool growth threshold percent, which determines above what percentage of usage
            /// the pool will be grown.
            /// </summary>
            public float InitialPoolGrowthThresholdPercent { get; set; } = 0.6f;

            /// <summary>
            /// Gets or sets the initial value for the pool growth rate percent, which is the percentage the pool grows by when it
            /// needs to be grown.
            /// </summary>
            public float InitialPoolGrowthRatePercent { get; set; } = 0.5f;

            /// <summary>
            /// Gets or sets the duration to wait before growing the pool to the minimum pool size during
            /// pool creation.
            /// </summary>
            public Duration CreationGrowthDelayDuration { get; set; }

            /// <summary>
            /// Gets or sets a duration to wait between creating each additional resource when growing
            /// the pool to the minimum pool size during pool creation.
            /// </summary>
            public Duration CreationGrowthIntervalDuration { get; set; } = Duration.FromMilliseconds(1000);

            /// <summary>
            /// Gets or sets a value which is the maximum for a random number generated which will become the
            /// startup growth delay. By using a random startup growth delay, you can stagger your startup so
            /// that we don't create too many resources at once.
            /// </summary>
            public Duration CreationGrowthDelayRandomMaxDuration { get; set; } = Duration.FromMilliseconds(10000);
        }
    }
}
