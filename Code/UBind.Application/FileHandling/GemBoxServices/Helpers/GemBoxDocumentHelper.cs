// <copyright file="GemBoxDocumentHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.FileHandling.GemBoxServices.Helpers
{
    using GemBox.Document;
    using GemBox.Document.MailMerging;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// A helper class for the GemBox.DocumentModel class
    /// </summary>
    public class GemBoxDocumentHelper
    {
        /// <summary>
        /// Loads a document from a content
        /// </summary>
        /// <param name="templateContent">The file content of an MS word document</param>
        /// <param name="loadOption">The load option</param>
        /// <returns>An instance of GemBox.DocumentModel loaded with the content</returns>
        public static DocumentModel LoadDocument(byte[] templateContent, LoadOptions loadOption)
        {
            try
            {
                using (var stream = new MemoryStream(templateContent))
                {
                    return DocumentModel.Load(stream, loadOption);
                }
            }
            catch (ArgumentNullException anex)
            {
                throw new ErrorException(
                    Errors.DocumentGeneration.LoadContentToMemoryStreamFailed(new JObject(), anex), anex);
            }
            catch (Exception ex)
            {
                throw new ErrorException(
                    Errors.DocumentGeneration.LoadContentToMemoryStreamFailed(new JObject(), ex), ex);
            }
        }

        /// <summary>
        /// Returns the clear options based on the parameters
        /// </summary>
        /// <param name="removeUnusedMergeFields">The flag for the removal of unused merge fields</param>
        /// <param name="removeRangesWhereAllMergeFieldsAreUnused">The flag for the removal of ranges which merge fields are unused</param>
        /// <param name="removeTablesWhereAllMergeFieldsAreUnused">The flag for the removal of tables which merge fields are unused</param>
        /// <param name="removeTableRowsWhereAllMergeFieldsAreUnused">The flag for the removal of table rows which merge fields are unused</param>
        /// <param name="removeParagraphsWhereAllMergeFieldsAreUnused">The flag for the removal of paragraphs which merge fields are unused</param>
        /// <returns>The clear options</returns>
        public static MailMergeClearOptions GetClearOptions(
            bool removeUnusedMergeFields,
            bool removeRangesWhereAllMergeFieldsAreUnused,
            bool removeTablesWhereAllMergeFieldsAreUnused,
            bool removeTableRowsWhereAllMergeFieldsAreUnused,
            bool removeParagraphsWhereAllMergeFieldsAreUnused)
        {
            var clearOptions = MailMergeClearOptions.None;
            if (removeUnusedMergeFields)
            {
                clearOptions = clearOptions | MailMergeClearOptions.RemoveUnusedFields;
            }

            if (removeRangesWhereAllMergeFieldsAreUnused)
            {
                clearOptions = clearOptions | MailMergeClearOptions.RemoveEmptyRanges;
            }

            if (removeTablesWhereAllMergeFieldsAreUnused)
            {
                clearOptions = clearOptions | MailMergeClearOptions.RemoveEmptyTables;
            }

            if (removeTableRowsWhereAllMergeFieldsAreUnused)
            {
                clearOptions = clearOptions | MailMergeClearOptions.RemoveEmptyTableRows;
            }

            if (removeParagraphsWhereAllMergeFieldsAreUnused)
            {
                clearOptions = clearOptions | MailMergeClearOptions.RemoveEmptyParagraphs;
            }

            return clearOptions;
        }
    }
}
