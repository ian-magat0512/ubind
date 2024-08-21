// <copyright file="FormDataHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json.Linq;
    using StackExchange.Profiling;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Entities;
    using UBind.Domain.Product.Component.Form;

    /// <summary>
    /// Helper class for working with FormData.
    /// </summary>
    public static class FormDataHelper
    {
        /// <summary>
        /// Gets a regular expression which matches File Data as submitted in a form, as part of Form Data.
        /// </summary>
        public static Regex FileDataRegex { get; } = new Regex("^([^\\:]+)\\:([^\\:]+)\\:([^\\:]*)\\:([^\\:]*)\\:([^\\:]*)\\:?([^\\:]*)?$");

        /// <summary>
        /// Attempts to use the question meta data to return the attachment field paths, but
        /// if it's an old form, then it finds the attachment fields by looking at their data.
        /// </summary>
        /// <param name="formDataSchema">The form data schema.</param>
        /// <param name="formData">The form data.</param>
        /// <returns>a list of fieldpaths for attachment fields.</returns>
        public static IEnumerable<string> GetQuestionAttachmentFieldPaths(FormDataSchema formDataSchema, JObject formData)
        {
            var questionMetaData = formDataSchema?.GetQuestionMetaData();
            if (questionMetaData != null && questionMetaData.Count > 0)
            {
                return questionMetaData.Where(x => x.DataType == DataType.Attachment).Select(x => x.Key);
            }
            else
            {
                var attachmentFieldPaths = new List<string>();
                if (formData != null)
                {
                    foreach (var jToken in formData.Descendants())
                    {
                        if (FileDataRegex.IsMatch(jToken.ToString()))
                        {
                            attachmentFieldPaths.Add(jToken.Path.Replace("formModel.", string.Empty));
                        }
                    }
                }

                return attachmentFieldPaths;
            }
        }

        /// <summary>
        /// Inserts the actual file data into the formdata so that it can be read by the webformapp.
        /// </summary>
        /// <param name="quote">The quote.</param>
        /// <param name="formModel">The form model.</param>
        /// <param name="fileAttachmentRepository">The file attachment repository.</param>
        public static void PopulateFormDataWithFileAttachmentContent(
            Quote quote,
            JObject formModel,
            IFileAttachmentRepository<QuoteFileAttachment> fileAttachmentRepository)
        {
            using (MiniProfiler.Current.Step(
                nameof(FormDataHelper) + "." + nameof(PopulateFormDataWithFileAttachmentContent)))
            {
                foreach (var item in formModel)
                {
                    JToken value = item.Value;
                    var includedAttachments =
                        quote.QuoteFileAttachments.Where(a => value.ToString().Contains(a.Id.ToString()));

                    foreach (var attachment in includedAttachments)
                    {
                        var attachmentId = attachment.Id;
                        var file =
                            fileAttachmentRepository.GetAttachmentContent(quote.Aggregate.TenantId, attachmentId);
                        if (file.HasValue)
                        {
                            var content = file.Value.GetFileContentString();
                            var newValue = string.Format("{0}:{1}", value.ToString(), content);
                            var newJToken = JToken.FromObject(newValue);
                            if (newJToken.Parent != null)
                            {
                                value.Replace(newJToken);
                            }
                        }
                    }
                }
            }
        }
    }
}
