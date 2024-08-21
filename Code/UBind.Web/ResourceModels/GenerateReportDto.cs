// <copyright file="GenerateReportDto.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using NodaTime;

    /// <summary>
    /// Model for generate report.
    /// </summary>
    public class GenerateReportDto
    {
        /// <summary>
        /// Gets or sets the from date of the report.
        /// </summary>
        public LocalDate From { get; set; }

        /// <summary>
        /// Gets or sets the to date of the report.
        /// </summary>
        public LocalDate To { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include the test data.
        /// </summary>
        public bool IncludeTestData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to save the generated report.
        /// </summary>
        public bool SaveGeneratedReport { get; set; }

        /// <summary>
        /// Gets or sets the environment.
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// Gets or sets the time zone Id.
        /// </summary>
        public string TimeZoneId { get; set; }
    }
}
