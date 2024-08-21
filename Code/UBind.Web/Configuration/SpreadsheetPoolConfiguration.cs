// <copyright file="SpreadsheetPoolConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Configuration
{
    using UBind.Application.FlexCel;

    public class SpreadsheetPoolConfiguration : ISpreadsheetPoolConfiguration
    {
        /// <summary>
        /// Gets or sets the minimum amount of time a workbook but be in existence before it can be reaped.
        /// </summary>
        public int MinimumResourceLifeMinutes { get; set; }

        /// <summary>
        /// Gets or sets the amount of time an instance should be unused for to be considered stale.
        /// </summary>
        public int ResourceUnusedStaleMinutes { get; set; }

        /// <summary>
        /// Gets or sets the time between when the reaper runs to check if any workbooks should be reaped.
        /// </summary>
        public int PoolReaperIntervalMinutes { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of instances that should be maintained in the pool
        /// for development environment workbooks.
        /// </summary>
        public int MinimumPoolSizeDevelopment { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of instances that should be maintained in the pool
        /// for staging environment workbooks.
        /// </summary>
        public int MinimumPoolSizeStaging { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of instances that should be maintained in the pool
        /// for production environment workbooks.
        /// </summary>
        public int MinimumPoolSizeProduction { get; set; }

        /// <summary>
        /// Gets or sets the amount to reduce the growth threshold by so that we grow the pool sooner in future.
        /// This is applicable when the pool is exhausted.
        /// </summary>
        public float PoolExhaustedGrowthThresholdPercentReduction { get; set; }

        /// <summary>
        /// Gets or sets the amount to increase the growth threshold by so that we grow the pool later in the future.
        /// This is applicabl when more than one pool member is reaped without having been used.
        /// </summary>
        public float ResourcesWastedGrowthThresholdPercentIncrease { get; set; }

        /// <summary>
        /// Gets or sets the amount to increase the growth rate by so that we grow the pool larger when it it's the threshold.
        /// This is applicable when the pool is exhausted.
        /// </summary>
        public float PoolExhaustedGrowthRatePercentIncrease { get; set; }

        /// <summary>
        /// Gets or sets the amount to reduce the growth rate percent by so that we grow the pool by a smaller amount in the future.
        /// This is applicable when more than one pool member is reaped without having been used.
        /// </summary>
        public float ResourcesWastedGrowthRatePercentReduction { get; set; }

        /// <summary>
        /// Gets or sets the initial value for the pool growth threshold percent, which determines above what percentage of usage
        /// the pool will be grown.
        /// </summary>
        public float InitialPoolGrowthThresholdPercent { get; set; }

        /// <summary>
        /// Gets or sets the initial value for the pool growth rate percent, which is the percentage the pool grows by when it
        /// needs to be grown.
        /// </summary>
        public float InitialPoolGrowthRatePercent { get; set; }

        /// <summary>
        /// Gets or sets the duration to wait before growing the pool to the minimum pool size right
        /// after a sync.
        /// </summary>
        public int SyncCreationGrowthDelayMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the duration to wait before growing the pool to the minimum pool size during
        /// normal pool creation.
        /// </summary>
        public int CreationGrowthDelayMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets a duration to wait between creating each additional resource when growing
        /// the pool to the minimum pool size during normal pool creation.
        /// </summary>
        public int CreationGrowthIntervalMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the duration to wait before growing the pool to the minimum pool size during
        /// application startup.
        /// </summary>
        public int StartupGrowthDelayMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets a duration to wait between creating each additional resource when growing
        /// the pool to the minimum pool size during application startup.
        /// </summary>
        public int StartupGrowthIntervalMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets a value which is the maximum for a random number generated which will become the
        /// startup growth delay. By using a random startup growth delay, you can stagger your startup so
        /// that we don't create too many resources at once.
        /// </summary>
        public int AdditionalStartupGrowthDelayRandomMaxMilliseconds { get; set; }
    }
}
