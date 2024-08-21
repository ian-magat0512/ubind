// <copyright file="DevReleaseInitializationArtefacts.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Releases
{
    using UBind.Application.FlexCel;
    using UBind.Domain;

    /// <summary>
    /// For holding the artefacts created as part of dev release initialization.
    /// </summary>
    public class DevReleaseInitializationArtefacts
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DevReleaseInitializationArtefacts"/> class.
        /// </summary>
        /// <param name="devRelease">The dev release.</param>
        /// <param name="quoteWorkbook">The workbook for the quote app.</param>
        /// <param name="claimWorkbook">The workbook for the claim app.</param>
        public DevReleaseInitializationArtefacts(
            DevRelease devRelease,
            IFlexCelWorkbook? quoteWorkbook,
            IFlexCelWorkbook? claimWorkbook)
        {
            this.DevRelease = devRelease;
            this.QuoteWorkbook = quoteWorkbook;
            this.ClaimWorkbook = claimWorkbook;
        }

        /// <summary>
        /// Gets the dev release.
        /// </summary>
        public DevRelease DevRelease { get; }

        /// <summary>
        /// Gets the workbook for the quote app.
        /// </summary>
        public IFlexCelWorkbook? QuoteWorkbook { get; }

        /// <summary>
        /// Gets the workbook for the claim app.
        /// </summary>
        public IFlexCelWorkbook? ClaimWorkbook { get; }
    }
}
