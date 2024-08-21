// <copyright file="ConfigurationFileModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using Humanizer;
    using UBind.Domain.Dto;

    /// <summary>
    /// Resource mode for Source File Dto.
    /// </summary>
    public class ConfigurationFileModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationFileModel"/> class.
        /// </summary>
        /// <param name="dto">The source file.</param>
        public ConfigurationFileModel(ConfigurationFileDto dto)
        {
            this.Path = dto.Path;
            this.SourceType = dto.SourceType;
            this.ResourceUrl = dto.ResourceUrl;
            this.FormType = dto.WebFormAppType.Humanize().ToLower();
            this.IsBrowserViewable = dto.IsBrowserViewable;
        }

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the root folder name of the file.
        /// </summary>
        public string FormType { get; set; }

        /// <summary>
        /// Gets or sets the source document type.
        /// </summary>
        public string SourceType { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        public string ResourceUrl { get; set; }

        public bool IsBrowserViewable { get; set; }
    }
}
