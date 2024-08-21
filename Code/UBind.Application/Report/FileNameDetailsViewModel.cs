// <copyright file="FileNameDetailsViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Report
{
    using DotLiquid;

    /// <summary>
    /// Filename details view model providing data for use in liquid report templates.
    /// </summary>
    public class FileNameDetailsViewModel : Drop
    {
        /// <summary>
        /// Gets or sets the name of the report.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the report.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the mime-type of the report.
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the from date of the report.
        /// </summary>
        public string FromDate { get; set; }

        /// <summary>
        /// Gets or sets the to date of the report.
        /// </summary>
        public string ToDate { get; set; }

        /// <summary>
        /// Gets or sets the current date of the report.
        /// </summary>
        public string GeneratedDate { get; set; }
    }
}
