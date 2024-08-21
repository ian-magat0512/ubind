// <copyright file="ReportReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Humanizer;
    using NodaTime;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Read model for the report.
    /// </summary>
    public class ReportReadModel : EntityReadModel<Guid>
    {
        private List<ReportSourceDataType> sourceDataList = new List<ReportSourceDataType>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportReadModel"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant id of the report.</param>
        /// <param name="organisationId">The organisation ID of the report.</param>
        /// <param name="reportId">The id of the report.</param>
        /// <param name="createdTimestamp">The created time of the report.</param>
        public ReportReadModel(
            Guid tenantId, Guid organisationId, Guid reportId, Instant createdTimestamp)
            : base(tenantId, reportId, createdTimestamp)
        {
            this.OrganisationId = organisationId;
            this.CreatedTimestamp = createdTimestamp;
        }

        // Parameterless constructor for EF.
        private ReportReadModel()
        {
        }

        /// <summary>
        /// Gets or sets the organisation ID of the report.
        /// </summary>
        public Guid OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the name of the report.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the report.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the products of the report.
        /// </summary>
        public virtual ICollection<Domain.Product.Product> Products { get; set; }

        /// <summary>
        /// Gets or sets the source data for the report.
        /// </summary>
        public string SourceData
        {
            get
            {
                return string.Join(",", this.sourceDataList.Select(pt => pt.Humanize(LetterCasing.Title)));
            }

            set
            {
                this.sourceDataList = value
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Dehumanize().ToEnumOrThrow<ReportSourceDataType>())
                    .ToList();
            }
        }

        /// <summary>
        /// Gets or sets gets or Sets the mime type of the report.
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the filename type of the report.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets the body type of the report.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the report is deleted.
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
