// <copyright file="MsWordMergeFieldType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.FileHandling.GemBoxServices.Enums
{
    using System.ComponentModel;

    /// <summary>
    /// Refers to the different types of MSWord merge fields.
    /// </summary>
    public enum MsWordMergeFieldType
    {
        /// <summary>
        /// A regular text merge field
        /// </summary>
        [Description("Text")]
        Text,

        /// <summary>
        /// An HTML string merge field
        /// </summary>
        [Description("Html")]
        Html,

        /// <summary>
        /// An image merge field
        /// </summary>
        [Description("Image")]
        Image,

        /// <summary>
        /// A word content merge field
        /// </summary>
        [Description("Word Content")]
        WordContent,

        /// <summary>
        /// An HTML content merge field
        /// </summary>
        [Description("HTML Content")]
        HtmlContent,

        /// <summary>
        /// An image content merge field
        /// </summary>
        [Description("HTML Content")]
        ImageContent,
    }
}
