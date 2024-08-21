// <copyright file="IDropGenerationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Report
{
    using System;
    using System.Collections.Generic;
    using NodaTime;
    using UBind.Domain;

    /// <summary>
    /// Service for view model creation for liquid template.
    /// </summary>
    public interface IDropGenerationService
    {
        /// <summary>
        /// Generate drop for report liquid template.
        /// </summary>
        /// <param name="tenantId">The tenant ID for report.</param>
        /// <param name="organisationId">The organisation ID for report.</param>
        /// <param name="productIds">The product Ids for report.</param>
        /// <param name="environment">The environment of the report.</param>
        /// <param name="sourceData">The source data of the report.</param>
        /// <param name="fromTimestamp">The from date of the report.</param>
        /// <param name="toTimestamp">The to date of the report.</param>
        /// <param name="includeTestData">Value whether to include test data.</param>
        /// <returns>Return the report view mode.</returns>
        Task<ReportBodyViewModel> GenerateReportDrop(
            Guid tenantId,
            Guid organisationId,
            IEnumerable<Guid> productIds,
            DeploymentEnvironment environment,
            IEnumerable<ReportSourceDataType> sourceData,
            Instant fromTimestamp,
            Instant toTimestamp,
            bool includeTestData);
    }
}
