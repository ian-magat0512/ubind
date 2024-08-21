// <copyright file="MultipartContentProperty.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Http
{
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Represents a mime-type/content pair, representing a part in multi-part content.
    /// </summary>
    public class MultipartContentProperty
    {
        /// <summary>
        /// Gets or sets the mime-type for this part in the multi-part content.
        /// </summary>
        public IProvider<Data<string>> ContentType { get; set; }

        /// <summary>
        /// Gets or sets the content for this part in the multi-part content.
        /// </summary>
        public ContentProvider Content { get; set; }
    }
}
