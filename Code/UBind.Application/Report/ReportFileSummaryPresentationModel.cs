// <copyright file="ReportFileSummaryPresentationModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Report
{
    using UBind.Domain;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Data transfer object for report file entity.
    /// </summary>
    public class ReportFileSummaryPresentationModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportFileSummaryPresentationModel"/> class.
        /// </summary>
        /// <param name="reportFileSummary">The report file summary object.</param>
        public ReportFileSummaryPresentationModel(ReportFileSummary reportFileSummary)
        {
            this.ReportFileId = reportFileSummary.Id.ToString();
            this.Filename = reportFileSummary.Filename;
            this.Size = reportFileSummary.Size;
            this.CreatedDateTime = reportFileSummary.CreatedTimestamp.ToExtendedIso8601String();
            this.MimeType = reportFileSummary.MimeType;
            this.Environment = reportFileSummary.Environment;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportFileSummaryPresentationModel"/> class.
        /// </summary>
        /// <param name="reportFile">The report file object.</param>
        public ReportFileSummaryPresentationModel(ReportFile reportFile)
        {
            // TODO: Add mimetype to ReportFile itself
            this.ReportFileId = reportFile.Id.ToString();
            this.Filename = reportFile.Filename;
            this.Size = reportFile.Size;
            this.CreatedDateTime = reportFile.CreatedTimestamp.ToExtendedIso8601String();
            this.MimeType = reportFile.MimeType;
            this.Environment = reportFile.Environment;
        }

        /// <summary>
        /// Gets the report file id.
        /// </summary>
        public string ReportFileId { get; private set; }

        /// <summary>
        /// Gets the report file name.
        /// </summary>
        public string Filename { get; private set; }

        /// <summary>
        /// Gets the report file size.
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Gets the created time.
        /// </summary>
        public string CreatedDateTime { get; private set; }

        /// <summary>
        /// Gets the mime-type of the file.
        /// </summary>
        public string MimeType { get; private set; }

        /// <summary>
        /// Gets the enivornment which the data for the report is for.
        /// </summary>
        public DeploymentEnvironment Environment { get; private set; }
    }
}
