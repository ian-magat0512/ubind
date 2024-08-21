// <copyright file="WordContentMergeField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling.GemBoxServices.MergeFields
{
    using GemBox.Document;
    using GemBox.Document.MailMerging;
    using GemBox.Document.Tables;
    using GemBox.Document.Tracking;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Providers.File;
    using UBind.Application.FileHandling.GemBoxServices.Enums;
    using UBind.Application.FileHandling.GemBoxServices.Extensions;
    using UBind.Application.FileHandling.GemBoxServices.Helpers;
    using UBind.Application.FileHandling.GemBoxServices.Models;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Represents a word content merge field.
    /// </summary>
    public class WordContentMergeField : MsWordMergeField
    {
        private readonly IEnumerable<ContentSourceFile> sourceFiles;
        private readonly List<string> wordDocumentFieldNames;

        public WordContentMergeField(IEnumerable<ContentSourceFile> sourceFiles, List<string> wordDocumentFieldNames)
        {
            this.sourceFiles = sourceFiles;
            this.wordDocumentFieldNames = wordDocumentFieldNames;
        }

        public override MsWordMergeFieldType Type => MsWordMergeFieldType.WordContent;

        public override void Merge(FieldMergingEventArgs e)
        {
            var fieldName = e.ExtractFieldName();
            var fileAlias = fieldName.ToCamelCase();
            var sourceFile = this.sourceFiles.Where(a => a.Alias == fileAlias).FirstOrDefault();
            if (sourceFile == null)
            {
                // We don't need to throw an error here because this should be treated as an unused merge field
                // and will be/not be cleared depending on the provided clear options
                return;
            }

            this.ThrowIfNotWordDocument(fieldName, sourceFile);

            var (affectedElement, affectedBlocks) = this.MergeAndGetAffectedElementsAndBlocks(e.Document, () =>
            {
                // Load the content document
                var contentDoc = GemBoxDocumentHelper.LoadDocument(sourceFile.File.Content, LoadOptions.DocxDefault);

                // It must be understood that each word document has its own character formatting set and one might differ from the other.
                // Our rule is, that all element from the content document must use the character formatting of the main
                // document unless the element was explicitly styled. If the element was explicitly styled, these styles
                // must be retained.
                // It must be also understood that each element contains two CharacterFormatting stored within its properties,
                // one of these is the template's set character formatting for that certain element ("Normal", "Headings 1", etc.),
                // and it is stored in the ParagraphFormat.Style.CharacterFormat property of the element (run), while the other is the actual
                // characater formatting that is in effect on that element and it is stored in the CharacterFormat property of the element.
                // If the element is not explictly styled, then the two CharacterFormats would be equal. In other words,
                // any difference between the properties of the two CharacterFormats means that the element has been explicitly styled
                // and these differences must be retained.
                // Before merging the content document to the main document, we have to create a copy of the content document's
                // CharacterFormat by storing its data to the CharacterFormatRevision property of each element.
                // We have to do this because we'll need this data to resolve the final CharacterFormat of the elements
                // after merging
                foreach (var section in contentDoc.Sections)
                {
                    this.SetFormattingRevisions(section.Blocks.ToList());

                    // Merge the content document to the main document
                    e.Field.Content.End.InsertRange(section.Blocks.Content);
                }
            });

            this.ResolveCharacterFormattingBasedOnCharacterFormatRevision(affectedBlocks);
            this.wordDocumentFieldNames.Add(e.FieldName);
        }

        private void ThrowIfNotWordDocument(string fieldName, ContentSourceFile? sourceFile)
        {
            bool isFileInvalidWordDocument = false;
            string fileName = sourceFile?.File.FileName.ToString() ?? "";

            if (string.IsNullOrEmpty(fileName))
            {
                isFileInvalidWordDocument = true;
            }

            string extension = Path.GetExtension(fileName);

            if (string.IsNullOrEmpty(extension))
            {
                isFileInvalidWordDocument = true; // No file extension found
            }

            // Check if the file extension is for Word documents
            var validExtensions = MsWordDocumentHelper.GetSupportedDocumentExtensionByThePlatform();
            isFileInvalidWordDocument = !validExtensions.Any(ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));

            if (isFileInvalidWordDocument)
            {
                var supportedExtensions = string.Join(", ", MsWordDocumentHelper
                    .GetSupportedDocumentExtensionByThePlatform()
                    .Select(a => $"\"{a.Substring(1)}\"")
                    .ToList());
                var errorData = new JObject
                {
                    { "contentAlias", sourceFile.Alias },
                    {
                        "supportedFileExtensions",
                        supportedExtensions
                    },
                };
                throw new ErrorException(
                    Errors.DocumentGeneration.WordDocumentFieldMerging.InvalidFileFormat(
                        fieldName,
                        fileName,
                        supportedExtensions,
                        errorData));
            }
        }

        private void SetFormattingRevisions(List<Block> blocks)
        {
            this.IterateBlockElements(blocks, (p) =>
            {
                // The template's default character formatting for this type of element
                var characterFormatFromTemplate = p.ParagraphFormat?.Style?.CharacterFormat;
                if (characterFormatFromTemplate != null)
                {
                    var blockContent = p.GetChildElements(true, ElementType.Run).ToList();
                    foreach (Run content in blockContent)
                    {
                        var revision = new CharacterFormatRevision(p.Document)
                        {
                            Author = "GemBoxMsWordEngineService",
                            Date = DateTime.Now,
                        };

                        // store it to the element as a CharacterFormatRevision for this will be used later
                        // to resolve the final CharacterFormat of the element
                        revision.CharacterFormat = characterFormatFromTemplate.Clone();
                        content.CharacterFormatRevision = revision;
                    }
                }
            });
        }

        private void IterateBlockElements(List<Block> blocks, Action<Paragraph> action)
        {
            foreach (var block in blocks)
            {
                if (block is Paragraph p)
                {
                    action(p);
                }
                else if (block is Table t)
                {
                    foreach (var row in t.Rows)
                    {
                        // Iterate all the cells within the table and resolve its character formatting
                        foreach (var cell in row.Cells)
                        {
                            this.IterateBlockElements(cell.Blocks.ToList(), action);
                        }
                    }
                }
            }
        }

        private void ResolveCharacterFormattingBasedOnCharacterFormatRevision(List<Block> blocks)
        {
            this.IterateBlockElements(blocks, (p) =>
            {
                var blockContent = p.GetChildElements(true, ElementType.Run).ToList();
                this.OverrideSpacing(p);
                foreach (Run content in blockContent)
                {
                    // The CharacterFormat from the main document
                    var characterFormatFromTemplate = p.ParagraphFormat?.Style?.CharacterFormat.Clone();
                    if (characterFormatFromTemplate != null)
                    {
                        // The content's CharacterFormat that we stored in the CharacterFormatRevision prior to the merge
                        var characterFormatFromRevision = content.CharacterFormatRevision?.CharacterFormat.Clone();
                        if (characterFormatFromRevision != null)
                        {
                            // Get the difference of the template's character format and the element's character format
                            // this difference are the styles that were explicitly applied to the element
                            var difference = GemBoxCharacterFormatModel
                                .FromCharacterFormatDifference(characterFormatFromRevision, content.CharacterFormat);
                            if (!difference.IsDefault())
                            {
                                // Apply the difference to the CharacterFormat from the main document
                                difference.ApplyToCharacterFormat(characterFormatFromTemplate);
                            }
                        }

                        // Override the element's CharacterFormat with the main document's
                        content.CharacterFormat = characterFormatFromTemplate;
                    }
                }
            });
        }
    }
}
