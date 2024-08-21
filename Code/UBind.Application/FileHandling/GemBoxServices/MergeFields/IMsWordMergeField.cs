// <copyright file="IMsWordMergeField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>
namespace UBind.Application.FileHandling.GemBoxServices.MergeFields
{
    using GemBox.Document.MailMerging;
    using UBind.Application.FileHandling.GemBoxServices.Enums;

    /// <summary>
    /// Defines the required contract for implementing a gembox ms word merge field.
    /// </summary>
    public interface IMsWordMergeField
    {
        /// <summary>
        /// Gets the type of the merge field.
        /// </summary>
        MsWordMergeFieldType Type { get; }

        /// <summary>
        ///  Merges the data into the merge field.
        /// </summary>
        /// <param name="e">The field merging event arguments</param>
        void Merge(FieldMergingEventArgs e);
    }
}
