// <copyright file="ReportFileNameViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Report
{
    using DotLiquid;

    /// <summary>
    /// Report filename view model providing data for use in liquid report templates.
    /// </summary>
    public class ReportFileNameViewModel : Drop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportFileNameViewModel"/> class.
        /// </summary>
        /// <param name="model">The name of the report.</param>
        public ReportFileNameViewModel(FileNameDetailsViewModel model)
        {
            this.Report = model;
        }

        /// <summary>
        /// Gets the report.
        /// Note: This property is nested inside the report filename view model in order to achieve
        ///     {{Report.[Property]}} variable inside the liquid template.
        /// </summary>
        public FileNameDetailsViewModel Report { get; private set; }
    }
}
