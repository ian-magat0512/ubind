// <copyright file="HtmlMergeField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.GemBoxServices.MergeFields
{
    using GemBox.Document;
    using GemBox.Document.MailMerging;
    using GemBox.Document.Tables;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Helper;
    using UBind.Application.FileHandling.GemBoxServices.Enums;
    using UBind.Application.FileHandling.GemBoxServices.Extensions;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Represents an HTML merge field.
    /// </summary>
    public class HtmlMergeField : MsWordMergeField
    {
        public override MsWordMergeFieldType Type => MsWordMergeFieldType.Html;

        public override void Merge(FieldMergingEventArgs e)
        {
            if (!e.IsValueFound)
            {
                return;
            }

            var value = this.GetStringOrThrow(e.Value, e.FieldName);
            this.MergeHtmlStringToField(e, value);
            e.Inline = null;
        }

        protected void MergeHtmlStringToField(FieldMergingEventArgs e, string htmlString)
        {
            var fieldName = e.ExtractFieldName();
            if (!HtmlStringHelper.IsValidHtml(htmlString))
            {
                var errorData = new JObject();
                errorData.Add("htmlString", htmlString.LimitLengthWithEllipsis(100));
                throw new ErrorException(
                    this.Type == MsWordMergeFieldType.Html
                        ? Errors.DocumentGeneration.HtmlFieldMerging.InvalidHtmlContent(fieldName, errorData)
                        : Errors.DocumentGeneration.HtmlContentFieldMerging.InvalidContent(fieldName, errorData));
            }

            var (affectedElements, affectedBlocks) = this.MergeAndGetAffectedElementsAndBlocks(e.Document, () =>
            {
                ContentPosition result = null;
                try
                {
                    result = e.Field.Content.End.LoadText(htmlString, new HtmlLoadOptions() { InheritCharacterFormat = true });

                    // ContentPosition.LoadText method only throws a NullReferenceException
                    // when the value of the HTML string contains an invalid HTML structure such
                    // as using <tr> element without <table>.
                }
                catch (NullReferenceException)
                {
                    var errorData = new JObject();
                    errorData.Add("htmlString", htmlString.LimitLengthWithEllipsis(100));
                    throw new ErrorException(
                        this.Type == MsWordMergeFieldType.Html
                        ? Errors.DocumentGeneration.HtmlFieldMerging.InvalidHtmlContent(fieldName, errorData)
                        : Errors.DocumentGeneration.HtmlContentFieldMerging.InvalidContent(fieldName, errorData));
                }

                // When the resulting ContentPosition's parent is of ElementType Run, its contents will not be rendered
                // in the output document and will appear as an empty element. This only occurs when the provided HTML string
                // does not contain any valid HTML block elements such as div, p, h1, table etc. We have to throw an
                // exception when this happens or this will behave as a silent failure.
                if (result == null || result.Parent.ElementType == ElementType.Run)
                {
                    var errorData = new JObject
                    {
                        { "htmlString", htmlString.LimitLengthWithEllipsis(100) }
                    };
                    throw new ErrorException(
                        this.Type == MsWordMergeFieldType.Html
                        ? Errors.DocumentGeneration.HtmlFieldMerging.InvalidHtmlContent(fieldName, errorData)
                        : Errors.DocumentGeneration.HtmlContentFieldMerging.InvalidContent(fieldName, errorData));
                }
            });

            this.ResolveCharacterFormatting(affectedBlocks, affectedElements, htmlString);
        }

        private string ResolveCharacterFormatting(List<Block> blocks, List<Element> modifiedElements, string htmlString)
        {
            foreach (var block in blocks)
            {
                // Fortunately, the resolved CharacterFormat of the element (from the template styling) that was not used, because
                // it was overriden by the default CSSs, is being stored in the block's paragraph format. GemBox overrides
                // this CharacterFormat by having this block's Content have its own CharacterFormat. What we'll do is we have to replace that
                // content's CharacterFormat with a clone of the CharacterFormat stored in the Paragraph's ParagraphFormat

                // If the block is Paragraph (<p/>, <div/>, <h1/>, <li/>, etc), it is stored in its ParagraphFormat.Style.CharacterFormat
                // property. If it is Table, we have to iterate the content of the table and override character formatting of
                // of the contents of any blocks within the table. The two other block elements, TableOfEntries and
                // BlockContentControl, are not used in holding contents that have CharacterFormat property
                if (block is Paragraph p)
                {
                    var characterFormatFromTemplate = p.ParagraphFormat?.Style?.CharacterFormat.Clone() ?? null;
                    this.OverrideSpacing(p);

                    if (characterFormatFromTemplate == null)
                    {
                        continue;
                    }

                    var blockContents = block.GetChildElements(true, ElementType.Run).ToList();
                    foreach (Run content in blockContents)
                    {
                        if (!modifiedElements.Contains(content))
                        {
                            continue;
                        }

                        if (content.CharacterFormat != characterFormatFromTemplate)
                        {
                            var characterFormatFromHtml = content.CharacterFormat.Clone();
                            content.CharacterFormat = characterFormatFromTemplate.Clone();
                            var tags = HtmlStringHelper.GetParentTagsOfText(htmlString, content.Text);
                            foreach (var tag in tags)
                            {
                                var t = HtmlStringHelper.RemoveTagAttributes(tag);
                                if (tag.Contains("<a ") && tag.Contains("href="))
                                {
                                    this.ApplyStyleForHyperinkTag(content);
                                }
                                else if (HtmlStringHelper.IsCharacterFormattingTag(tag))
                                {
                                    this.ApplyStyleOnSupportedCharacterFormattingTag(content, t);
                                }

                                this.ApplySupportedAttributesOnHtmlTag(content, characterFormatFromHtml, tag);
                                this.CheckAndApplyListItemFormat(p, content, t);
                            }

                            // Remove the text from html string in case of multiple occurrence of the text
                            // in the html string
                            int indexOfRemoval = htmlString.IndexOf(content.Text);
                            if (indexOfRemoval >= 0)
                            {
                                htmlString = htmlString.Remove(indexOfRemoval, content.Text.Length);
                            }
                        }
                    }
                }
                else if (block is Table t)
                {
                    this.ApplyDocumentStylesToHtmlTable(htmlString, t);

                    foreach (var row in t.Rows)
                    {
                        // Iterate all the cells within the table and resolve its character formatting
                        foreach (var cell in row.Cells)
                        {
                            htmlString = this.ResolveCharacterFormatting(cell.Blocks.ToList(), modifiedElements, htmlString);
                        }
                    }
                }
            }

            return htmlString;
        }

        private void ApplyStyleForHyperinkTag(Run content)
        {
            var hyperlinkStyle = content.Document.Styles
                                                    .Where(a => a.StyleType == StyleType.Character && a.Name == "Hyperlink")
                                                    .Select(a => (CharacterStyle)a)
                                                    .FirstOrDefault();
            if (hyperlinkStyle != null)
            {
                content.CharacterFormat = hyperlinkStyle.CharacterFormat.Clone();
                content.CharacterFormat.Style = hyperlinkStyle;
            }
            else
            {
                content.CharacterFormat.UnderlineStyle = UnderlineType.Single;
                content.CharacterFormat.UnderlineColor = Color.Blue;
                content.CharacterFormat.FontColor = Color.Blue;
            }
        }

        /// <summary>
        /// If the tag is a supported CharacterFormatting tag, then apply its
        /// corresponding formatting
        /// </summary>
        /// <param name="content"></param>
        /// <param name="t"></param>
        private void ApplyStyleOnSupportedCharacterFormattingTag(Run content, string t)
        {
            switch (t)
            {
                case "<b>":
                case "<bold>":
                case "<strong>":
                    content.CharacterFormat.Bold = true;
                    break;
                case "<i>":
                case "<em>":
                    content.CharacterFormat.Italic = true;
                    break;
                case "<u>":
                    content.CharacterFormat.UnderlineStyle = UnderlineType.Single;
                    break;
                case "<s>":
                case "<del>":
                case "<strike>":
                    content.CharacterFormat.Strikethrough = true;
                    break;
                case "<sub>":
                    content.CharacterFormat.Subscript = true;
                    break;
                case "<sup>":
                    content.CharacterFormat.Superscript = true;
                    break;
            }
        }

        /// <summary>
        /// Let's check the tag for style attributes and check
        /// for any relevant character formatting attributes, if there are any,
        /// then use the overridden character format since it contains all the resolved
        /// css styling from the html content
        /// </summary>
        /// <param name="content"></param>
        /// <param name="characterFormatFromHtml"></param>
        /// <param name="tag"></param>
        private void ApplySupportedAttributesOnHtmlTag(Run content, CharacterFormat characterFormatFromHtml, string tag)
        {
            var attributes = HtmlStringHelper.ExtractStyleAttributes(tag);
            foreach (var attribute in attributes)
            {
                switch (attribute)
                {
                    case "color":
                        content.CharacterFormat.FontColor = characterFormatFromHtml.FontColor;
                        break;
                    case "font-family":
                        content.CharacterFormat.FontName = characterFormatFromHtml.FontName;
                        break;
                    case "font-size":
                        content.CharacterFormat.Size = characterFormatFromHtml.Size;
                        break;
                    case "font-style":
                        content.CharacterFormat.Italic = characterFormatFromHtml.Italic;
                        break;
                    case "font-weight":
                        content.CharacterFormat.Bold = characterFormatFromHtml.Bold;
                        break;
                    case "text-decoration":
                        content.CharacterFormat.UnderlineStyle = characterFormatFromHtml.UnderlineStyle;
                        content.CharacterFormat.UnderlineColor = characterFormatFromHtml.UnderlineColor;
                        break;
                    case "text-transform":
                        content.CharacterFormat.AllCaps = characterFormatFromHtml.AllCaps;
                        content.CharacterFormat.SmallCaps = characterFormatFromHtml.SmallCaps;
                        break;
                }
            }
        }

        private void CheckAndApplyListItemFormat(Paragraph p, Run content, string t)
        {
            if (t == "<li>")
            {
                if (p.ListFormat != null && p.ListFormat.ListLevelFormat != null)
                {
                    p.CharacterFormatForParagraphMark = content.CharacterFormat.Clone(true);
                }
            }
        }

        /// <summary>
        /// Check if the html string has a table element. If it has, then the table
        /// was generated from the html and its properties must be overridden.
        /// </summary>
        /// <param name="htmlString"></param>
        /// <param name="t"></param>
        private void ApplyDocumentStylesToHtmlTable(string htmlString, Table t)
        {
            var tableTag = HtmlStringHelper.FindAllTags(htmlString, "table").FirstOrDefault();
            if (tableTag != null
                && (t.TableFormat.Style == null
                || t.TableFormat.Style.Name.Equals("Normal Table", StringComparison.OrdinalIgnoreCase)))
            {
                var tableStyle = (TableStyle)t.Document.Styles.GetOrAdd(StyleTemplateType.TableGrid);
                t.TableFormat = tableStyle.TableFormat.Clone();
                t.TableFormat.Style = tableStyle;
            }
        }
    }
}
