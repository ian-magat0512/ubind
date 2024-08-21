// <copyright file="ReportPresentationModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Report
{
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Data transfer object for report entity.
    /// </summary>
    public class ReportPresentationModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportPresentationModel"/> class.
        /// </summary>
        /// <param name="reportDetails">The report aggregate.</param>
        public ReportPresentationModel(ReportDetailsReadModel reportDetails)
        {
            this.Id = reportDetails.Report.Id.ToString();
            this.Name = reportDetails.Report.Name;
            this.Description = reportDetails.Report.Description;
            this.SourceData = reportDetails.Report.SourceData;
            this.MimeType = reportDetails.Report.MimeType;
            this.Filename = reportDetails.Report.Filename;
            this.Body = reportDetails.Report.Body;
            this.IsDeleted = reportDetails.Report.IsDeleted;
            this.CreatedDateTime = reportDetails.Report.CreatedTimestamp.ToExtendedIso8601String();
            this.LastModifiedDateTime = reportDetails.Report.LastModifiedTimestamp.ToExtendedIso8601String();

            if (reportDetails.Report.Products != null)
            {
                this.Products = reportDetails.Report.Products
                    .Where(p => p.Details.Deleted == false)
                    .Select(p => new ProductPresentationModel(
                            p.TenantId,
                            p.Id,
                            p.Details.Alias,
                            p.Details.Name))
                    .ToList();
            }
        }

        /// <summary>
        /// Gets the id of the report.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the name of the report.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description of the report.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the products of the report.
        /// </summary>
        public List<ProductPresentationModel> Products { get; private set; }

        /// <summary>
        /// Gets the source data of the report.
        /// </summary>
        public string SourceData { get; private set; }

        /// <summary>
        /// Gets the mime-type of the report.
        /// </summary>
        public string MimeType { get; private set; }

        /// <summary>
        /// Gets the filename of the report.
        /// </summary>
        public string Filename { get; private set; }

        /// <summary>
        /// Gets the body of the report.
        /// </summary>
        public string Body { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the report is deleted.
        /// </summary>
        public bool IsDeleted { get; private set; }

        /// <summary>
        /// Gets the created datetime.
        /// </summary>
        public string CreatedDateTime { get; private set; }

        /// <summary>
        /// Gets the last modified datetime.
        /// </summary>
        public string LastModifiedDateTime { get; private set; }
    }
}
