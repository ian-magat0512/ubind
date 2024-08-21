// <copyright file="ReportFilePresentationModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Report
{
    using UBind.Domain;

    /// <summary>
    /// Model for presenting a report file, including it's byte data.
    /// </summary>
    public class ReportFilePresentationModel : ReportFileSummaryPresentationModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportFilePresentationModel"/> class.
        /// </summary>
        /// <param name="reportFile">The report file object.</param>
        public ReportFilePresentationModel(ReportFile reportFile)
            : base(reportFile)
        {
            this.Content = reportFile.Content;
        }

        /// <summary>
        /// Gets the content of the file.
        /// </summary>
        public byte[] Content { get; private set; }
    }
}
