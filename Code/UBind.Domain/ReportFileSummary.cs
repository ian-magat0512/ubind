// <copyright file="ReportFileSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// Represents the summary information for a report file, and doesn't include the actual raw byte data.
    /// </summary>
    public class ReportFileSummary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportFileSummary"/> class.
        /// </summary>
        /// <param name="reportFileId">The id of the report file.</param>
        /// <param name="reportId">The id of the report.</param>
        /// <param name="environment">The environment the report data is from.</param>
        /// <param name="filename">The name of the file.</param>
        /// <param name="size">The size of the file in bytes.</param>
        /// <param name="mimeType">The mime type of the file.</param>
        /// <param name="timeCreated">The time this entity was created.</param>
        public ReportFileSummary(
            Guid reportFileId,
            Guid reportId,
            DeploymentEnvironment environment,
            string filename,
            int size,
            string mimeType,
            Instant timeCreated)
        {
            this.Id = reportFileId;
            this.CreatedTimestamp = timeCreated;
            this.Filename = filename;
            this.ReportId = reportId;
            this.Size = size;
            this.MimeType = mimeType;
            this.Environment = environment;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportFileSummary"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// </remarks>
        public ReportFileSummary()
        {
        }

        /// <summary>
        /// Gets or sets the entity's unique identifier.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the instant in time the entity was created.
        /// </summary>
        public Instant CreatedTimestamp
        {
            get { return Instant.FromUnixTimeTicks(this.CreatedTicksSinceEpoch); }
            set { this.CreatedTicksSinceEpoch = value.ToUnixTimeTicks(); }
        }

        /// <summary>
        /// Gets or sets the entity created time (in ticks since Epoch).
        /// </summary>
        /// <remarks> Primitive typed property for EF to store created time.</remarks>
        public long CreatedTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets filename of an ReportFile.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets the size of the report file.
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the size of the report file.
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the id report.
        /// </summary>
        public Guid ReportId { get; set; }

        /// <summary>
        /// Gets or sets the enivornment which the data for the report is for.
        /// </summary>
        public DeploymentEnvironment Environment { get; set; }
    }
}
