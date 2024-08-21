// <copyright file="FieldMergingEventArgsExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.GemBoxServices.Extensions
{
    using GemBox.Document.MailMerging;
    using UBind.Application.FileHandling.GemBoxServices.Enums;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Extension methods for FieldMergingEventArgs.
    /// </summary>
    public static class FieldMergingEventArgsExtensions
    {
        /// <summary>
        /// Parses the full field name to get the merge field type.
        /// </summary>
        /// <param name="e">The field merging event argument</param>
        /// <returns>The resolved field type</returns>
        public static MsWordMergeFieldType GetMsWordMergeFieldType(this FieldMergingEventArgs e)
        {
            if (!e.FieldName.Contains(':'))
            {
                return MsWordMergeFieldType.Text;
            }

            var splitText = e.FieldName.Split(':');
            var fieldType = splitText[0];
            var fieldName = splitText[1];
            if (!Enum.TryParse(fieldType.Trim(), out MsWordMergeFieldType result))
            {
                throw new ErrorException(
                    UBind.Domain.Errors.DocumentGeneration.FieldMerging.InvalidMergeFieldType(fieldName, null));
            }

            return result;
        }

        /// <summary>
        /// Extracts the field name from the full field name.
        /// </summary>
        /// <param name="e">The field merging event argument</param>
        /// <returns>The field name without the merge field type</returns>
        public static string ExtractFieldName(this FieldMergingEventArgs e)
        {
            if (!e.FieldName.Contains(':'))
            {
                return e.FieldName;
            }

            return e.FieldName.Split(':')[1];
        }
    }
}
