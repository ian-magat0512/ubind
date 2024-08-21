// <copyright file="MsWordMergeField.cs" company="uBind">
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
    using UBind.Application.FileHandling.GemBoxServices.Enums;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Represents the base class for all merge fields.
    /// </summary>
    public class MsWordMergeField : IMsWordMergeField
    {
        public virtual MsWordMergeFieldType Type => MsWordMergeFieldType.Text;

        public virtual void Merge(FieldMergingEventArgs e)
        {
            // Do nothing because this is a regular merge field that does not need any special handling
        }

        protected (List<Element>, List<Block>) MergeAndGetAffectedElementsAndBlocks(DocumentModel document, Action mergeAction)
        {
            // Get all blocks in the document prior to the merge
            var presentBlocks = this.GetAllBlocks(document).ToList();

            // Get all elements in the document prior to the merge
            var presentElements = presentBlocks.SelectMany(a => a.GetChildElements(true)).ToList();

            // Perform the merge
            mergeAction();

            // Get all blocks after the merge
            var postMergeBlocks = this.GetAllBlocks(document);

            // Get all elements that is either new or has been modified after the merge
            var affectedElements = postMergeBlocks
                .SelectMany(a => a.GetChildElements(true))
                .Where(a => !presentElements.Any(pb => pb.Equals(a)))
                .ToList();

            // Get all newly added blocks and all blocks that cointain new or modified elements after the merge
            return (affectedElements, postMergeBlocks
                .Where(
                    a => !presentBlocks.Any(pb => pb.Equals(a))
                    || affectedElements.Any(el => el.GetParentElements().Any(elp => elp.Equals(a))))
                .ToList());
        }

        protected List<Block> GetAllBlocks(DocumentModel document)
        {
            var blocks = new List<Block>();
            foreach (var sec in document.Sections)
            {
                blocks.AddRange(sec.Blocks);
            }

            return blocks;
        }

        protected string GetStringOrThrow(object obj, string fieldName)
        {
            if (this.IsObjectNull(obj))
            {
                var errorData = new JObject();
                throw new ErrorException(
                        Errors.DocumentGeneration.FieldMerging.NullMergeFieldValue(fieldName, errorData));
            }

            if (obj is JToken token && token.Type == JTokenType.String)
            {
                return token.ToString();
            }
            else
            {
                var type = obj is JToken t ? t.Type.ToString() : obj.GetType().Name;
                throw new ErrorException(
                        Errors.DocumentGeneration.HtmlFieldMerging.InvalidFieldValueDataType(fieldName, type, new JObject()));
            }
        }

        protected bool IsObjectNull(object obj)
        {
            if (obj == null)
            {
                return true;
            }
            else if (obj is JToken token && token.Type == JTokenType.Null)
            {
                return true;
            }

            return false;
        }

        protected void OverrideSpacing(Paragraph p)
        {
            ParagraphFormat? format = null;
            var parentTable = p.GetParentElements(ElementType.Table).LastOrDefault();
            if (parentTable != null)
            {
                var tb = (Table)parentTable;
                format = tb.TableFormat?.Style?.ParagraphFormat;
            }
            else if (p.ListFormat != null && p.ListFormat.ListLevelFormat != null)
            {
                var listParagraphStyle = (ParagraphStyle)p.Document.Styles.GetOrAdd(StyleTemplateType.ListParagraph);
                format = listParagraphStyle.ParagraphFormat.Clone(true);
                p.ParagraphFormat.Style = listParagraphStyle;
                if (p.ListFormat.ListLevelNumber > 0)
                {
                    p.ParagraphFormat.LeftIndentation = p.ListFormat.ListLevelFormat.ParagraphFormat.LeftIndentation;
                }
            }
            else
            {
                format = p.ParagraphFormat?.Style?.ParagraphFormat;
            }

            if (format != null)
            {
                p.ParagraphFormat.SpaceBefore = format.SpaceBefore;
                p.ParagraphFormat.SpaceAfter = format.SpaceAfter;
                p.ParagraphFormat.NoSpaceBetweenParagraphsOfSameStyle = format.NoSpaceBetweenParagraphsOfSameStyle;
                p.ParagraphFormat.LineSpacing = format.LineSpacing;
                p.ParagraphFormat.LineSpacingRule = format.LineSpacingRule;
                p.ParagraphFormat.KeepLinesTogether = format.KeepLinesTogether;
                p.ParagraphFormat.Alignment = format.Alignment;
                p.ParagraphFormat.BackgroundColor = format.BackgroundColor;
                p.ParagraphFormat.WordWrap = format.WordWrap;
                p.ParagraphFormat.OutlineLevel = format.OutlineLevel;
                p.ParagraphFormat.SuppressLineNumbers = format.SuppressLineNumbers;
                p.ParagraphFormat.PageBreakBefore = format.PageBreakBefore;
                p.ParagraphFormat.KeepWithNext = format.KeepWithNext;
                p.ParagraphFormat.MirrorIndents = format.MirrorIndents;
                p.ParagraphFormat.WidowControl = format.WidowControl;
            }
        }
    }
}
