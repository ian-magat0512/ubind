// <copyright file="ExportModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    /// <summary>
    /// Options for export type and for now, we only support application type.
    /// </summary>
    public enum ExportType
    {
        /// <summary>
        /// Option for application type exports.
        /// </summary>
        Application,
    }

    /// <summary>
    /// Model that identifies query filter options.
    /// </summary>
    public enum FilterBy
    {
        /// <summary>
        /// None of the options.
        /// </summary>
        None = 0,

        /// <summary>
        /// Option for those who has policies.
        /// </summary>
        HasPolicy,

        /// <summary>
        /// Option for those who has invoices.
        /// </summary>
        HasInvoice,

        /// <summary>
        /// Option for those who has payments.
        /// </summary>
        HasPayment,

        /// <summary>
        /// Option for those who has submissions.
        /// </summary>
        HasSubmission,
    }

    /// <summary>
    /// Enumerations for output format.
    /// </summary>
    public enum OutputFormat
    {
        /// <summary>
        /// Option for comma separated values file which allows data to be saved in a table structured format.
        /// </summary>
        CSV,

        /// <summary>
        /// Option for lightweight data-interchange format.
        /// </summary>
        JSON,
    }

    /// <summary>
    /// Periodic export of data model.
    /// </summary>
    public class ExportModel : DateRangeModel
    {
        /// <summary>
        /// Gets or sets the application type.
        /// </summary>
        public ExportType ExportType { get; set; }

        /// <summary>
        ///  Gets or sets the filter.
        /// </summary>
        public FilterBy FilterBy { get; set; }

        /// <summary>
        /// Gets or sets the output format.
        /// </summary>
        public OutputFormat OutputFormat { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to return a test data.
        /// </summary>
        public bool IncludeTestData { get; set; }
    }
}
