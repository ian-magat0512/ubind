// <copyright file="ReportCreateModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using UBind.Domain;

    /// <summary>
    /// Model for report.
    /// </summary>
    public class ReportCreateModel
    {
        /// <summary>
        /// Gets or sets deployment environment.
        /// </summary>
        public DeploymentEnvironment Environment { get; set; }

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
        public Guid[] ProductIds { get; set; }

        /// <summary>
        /// Gets or sets the source data of the report.
        /// </summary>
        public string SourceData { get; set; }

        /// <summary>
        /// Gets or sets the mime-type of the report.
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the filename of the report.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Gets or sets the body of the report.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the report was deleted.
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
