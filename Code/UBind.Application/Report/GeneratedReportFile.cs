// <copyright file="GeneratedReportFile.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Report
{
    /// <summary>
    /// Generated report file.
    /// </summary>
    public class GeneratedReportFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratedReportFile"/> class.
        /// </summary>
        /// <param name="filename">The filename of the generated report.</param>
        /// <param name="content">The content of the generated report.</param>
        /// <param name="mimeType">The mimetype of the generated report.</param>
        public GeneratedReportFile(string filename, byte[] content, string mimeType)
        {
            this.Filename = filename;
            this.Content = content;
            this.MimeType = mimeType;
        }

        /// <summary>
        /// Gets the filename of the generated report.
        /// </summary>
        public string Filename { get; }

        /// <summary>
        /// Gets the content of the generated report.
        /// </summary>
        public byte[] Content { get; }

        /// <summary>
        /// Gets the mime type of the generated report.
        /// </summary>
        public string MimeType { get; }
    }
}
