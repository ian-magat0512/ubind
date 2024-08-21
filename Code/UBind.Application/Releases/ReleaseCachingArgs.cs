// <copyright file="ReleaseCachingArgs.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Releases
{
    using System;
    using UBind.Application.FlexCel;

    /// <summary>
    /// Arguments for events emitted when a release is cached in the global cache.
    /// </summary>
    public class ReleaseCachingArgs : EventArgs
    {
        private ReleaseCachingArgs(
            ActiveDeployedRelease release,
            IFlexCelWorkbook? quoteWorkbook,
            IFlexCelWorkbook? claimWorkbook)
        {
            this.Release = release;
            this.QuoteWorkbook = quoteWorkbook;
            this.ClaimWorkbook = claimWorkbook;
        }

        /// <summary>
        /// Gets the release which was cached or uncached.
        /// </summary>
        public ActiveDeployedRelease Release { get; }

        /// <summary>
        /// Gets a quote workbook for the release if available, otherwise null.
        /// </summary>
        public IFlexCelWorkbook? QuoteWorkbook { get; }

        /// <summary>
        /// Gets a claim workbook for the release if available, otherwise null.
        /// </summary>
        public IFlexCelWorkbook? ClaimWorkbook { get; }

        /// <summary>
        /// Create event arguments for when a cached release has seed workbooks available.
        /// </summary>
        /// <param name="release">The release.</param>
        /// <param name="quoteWorkbook">The quote workbook.</param>
        /// <param name="claimWorkbook">Theh claim workbook.</param>
        /// <returns>The event arguments.</returns>
        public static ReleaseCachingArgs CreateWithSeedWorkbooks(
            ActiveDeployedRelease release,
            IFlexCelWorkbook? quoteWorkbook,
            IFlexCelWorkbook? claimWorkbook) =>
            new ReleaseCachingArgs(release, quoteWorkbook, claimWorkbook);

        /// <summary>
        /// Create event arguments for when a cached release has no seed workbooks available.
        /// </summary>
        /// <param name="release">The release.</param>
        /// <returns>The event arguments.</returns>
        public static ReleaseCachingArgs CreateWithoutSeedWorkbooks(ActiveDeployedRelease release) =>
            new ReleaseCachingArgs(release, null, null);
    }
}
