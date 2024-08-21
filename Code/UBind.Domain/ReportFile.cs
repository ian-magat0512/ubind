// <copyright file="ReportFile.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// Represents a report file, including all of the file raw byte data.
    /// </summary>
    public class ReportFile : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportFile"/> class.
        /// </summary>
        /// <param name="reportFileId">The id of the report file.</param>
        /// <param name="reportId">The id of the report.</param>
        /// <param name="environment">The environment the report data is from.</param>
        /// <param name="filename">The name of the file.</param>
        /// <param name="content">The file contents.</param>
        /// <param name="mimeType">The mime type of the file.</param>
        /// <param name="timeCreated">The time this entity was created.</param>
        public ReportFile(
            Guid reportFileId,
            Guid reportId,
            DeploymentEnvironment environment,
            string filename,
            byte[] content,
            string mimeType,
            Instant timeCreated)
            : base(reportFileId, timeCreated)
        {
            this.Filename = filename;
            this.ReportId = reportId;
            this.Size = content.Length;
            this.MimeType = mimeType;
            this.Environment = environment;
            this.Content = content;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportFile"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// </remarks>
        public ReportFile()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Gets filename of an ReportFile.
        /// </summary>
        public string Filename { get; private set; }

        /// <summary>
        /// Gets the size of the report file.
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Gets the size of the report file.
        /// </summary>
        public string MimeType { get; private set; }

        /// <summary>
        /// Gets the id report.
        /// </summary>
        public Guid ReportId { get; private set; }

        /// <summary>
        /// Gets the enivornment which the data for the report is for.
        /// </summary>
        public DeploymentEnvironment Environment { get; private set; }

        /// <summary>
        /// Gets the file content of an ReportFile.
        /// </summary>
        public byte[] Content { get; private set; }
    }
}
