// <copyright file="DocumentGeneration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

#pragma warning disable SA1600
#pragma warning disable SA1118 // Parameter should not span multiple lines

namespace UBind.Domain;

using System;
using System.Net;
using Newtonsoft.Json.Linq;

/// <summary>
/// Allows enumeration of all document generation errors as outlined here: https://enterprisecraftsmanship.com/posts/advanced-error-handling-techniques/.
/// </summary>
public static partial class Errors
{
    /// <summary>
    /// Document Generation errors.
    /// </summary>
    public static class DocumentGeneration
    {
        public static Error LoadContentToMemoryStreamFailed(JObject data, Exception ex) =>
            new Error(
                "load.content.to.memory.stream.failed",
                "Loading content to a memory stream failed",
                "There was an error in loading the content of the source template into a memory stream "
                + $"because of this error '{ex.Message}'. "
                + "Please contact customer support for assistance. We apologise for the inconvenience.",
                HttpStatusCode.InternalServerError,
                null,
                data);

        public static Error GenerateFinalOutputFailed(JObject data) =>
            new Error(
                "generate.output.failed",
                "Generating output failed",
                "There was an error in generating the final output of the merge field operation. It is possible that "
                + $"the source file's content is not supported or is corrupt, or the data was not merged successfully "
                + "with the source file. Please contact customer support for assistance. "
                + "We apologise for the inconvenience.",
                HttpStatusCode.InternalServerError,
                null,
                data);

        public static class FieldMerging
        {
            public static Error InvalidFieldValueDataType(string fieldName, string type, JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    $"merge.field.value.data.type.invalid",
                    "The merge field value had an invalid data type",
                    $"When trying to read the source value for the \"{fieldName}\" merge field, the attempt failed "
                        + $"because the value was of an unsupported data type (\"{type.ToLower()}\"). To resolve this issue, "
                        + $"please ensure that the value provided for the \"{fieldName}\" merge field is "
                        + "of type \"string\". If you need further assistance, please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data);

            public static Error NullMergeFieldValue(string fieldName, JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "merge.field.value.null",
                    "The merge field value was null",
                    $"When trying to read the source value for the \"{fieldName}\" merge field, the attempt failed because "
                        + $"the value was null. To resolve this issue, please ensure that the value provided for the "
                        + $"you provide for the \"{fieldName}\" merge fields is not null. If you need further assistance, "
                        + "please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data);

            public static Error InvalidMergeFieldType(string fieldName, JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "merge.field.type.invalid",
                    "The merge field type was invalid",
                    $"When trying to resolve the merge field type, the attempt failed because "
                        + $"provided type was invalid. To resolve this issue, please ensure that the prefix "
                        + $"you provide for the \"{fieldName}\" merge field is valid (\"Html:\", \"Image:\", \"WordContent:\" "
                        + $"\"HtmlContent:\", \"ImageContent:\" or \"\"). If you need further assistance, "
                        + "please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data);
        }

        public static class HtmlFieldMerging
        {
            public static Error InvalidFieldValueDataType(string fieldName, string type, JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    $"merge.field.value.html.data.type.invalid",
                    "The merge field value had an invalid data type",
                    $"When trying to read the source value for the \"{fieldName}\" HTML merge field, the attempt failed "
                        + $"because the value was of an unsupported data type (\"{type.ToLower()}\"). To resolve this issue, "
                        + $"please ensure that the value provided for the \"{fieldName}\" HTML merge field is "
                        + "of type \"string\". If you need further assistance, please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data);

            public static Error InvalidHtmlContent(string fieldName, JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "merge.field.value.html.content.invalid",
                    "The merge field value contained invalid HTML",
                    $"When trying to read the source value for the \"{fieldName}\" HTML merge field, the attempt failed because "
                        + "the value did not contain valid HTML with a supported structure. To resolve this issue, please "
                        + $"ensure that the value provided for  the \"{fieldName}\" HTML merge field is valid HTML and that "
                        + "it contains at least one block element such as div, p, h1, h2 or similar. If you need further "
                        + "assistance, please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data);
        }

        public static class ImageFieldMerging
        {
            public static Error InvalidFieldValueDataType(string fieldName, string type, JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    $"merge.field.value.image.data.type.invalid",
                    "The merge field value had an invalid data type",
                    $"When trying to read the source value for the \"{fieldName}\" image merge field, the attempt failed "
                        + $"because the value was of an unsupported data type (\"{type.ToLower()}\"). To resolve this issue, "
                        + $"please ensure that the value provided for the \"{fieldName}\" image merge field is "
                        + "of type \"string\". If you need further assistance, please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data);

            public static Error InvalidImageFieldValueImageData(string fieldName, JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "merge.field.value.image.data.invalid",
                    "Invalid merge field value structure",
                    GenerateErrorMessage(
                        fieldName,
                        $"the image data is structured incorrectly",
                        "please ensure that the image data you provide for the image merge field is structured as follows: "
                        + "\"filename:mimeType:attachmentId:imageWidth:imageHeight\""),
                    HttpStatusCode.BadRequest,
                    data);

            public static Error InvalidImageFieldValueMimeType(string fieldName, string mimeType, JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "merge.field.value.image.mime.type.invalid",
                    "The source file had an unsupported mime type",
                    GenerateErrorMessage(
                        fieldName,
                        $"the source file has a mime type (\"{mimeType}\") that is not supported",
                        "please ensure that the attached image file has a supported mime type "
                        + "(\"image/gif\", \"image/jpeg\" or \"image/png\")"),
                    HttpStatusCode.BadRequest,
                    data);

            public static Error InvalidImageFieldValueFileFormat(string fieldName, string fileName, JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "merge.field.value.image.file.format.invalid",
                    "The source file had an unsupported filename extension",
                    GenerateErrorMessage(
                        fieldName,
                        $"the source file (\"{fileName}\") has a filename extension that is not supported",
                        "please ensure that the attached image file has a supported filename " +
                        "extension (\"png\", \"jpg\", \"jpeg\" or \"gif\")"),
                    HttpStatusCode.BadRequest,
                    data);

            public static Error InvalidImageFieldValueImageSize(
                string fieldName,
                string width,
                string height,
                JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "merge.field.value.image.size.invalid",
                    "The image size provided is invalid",
                    GenerateErrorMessage(
                        fieldName,
                        $"the provided width (\"{width}\") and/or height (\"{height}\") of the image from image data "
                        + $"is invalid",
                        "please ensure that the provided image width and height are non-negative numbers and the image data "
                        + "is structured as follows: \"filename:mimeType:attachmentId:imageWidth:imageHeight\""),
                    HttpStatusCode.BadRequest,
                    data);

            public static Error AttachmentIdNotFound(string fieldName, JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "merge.field.value.image.attachment.id.not.found",
                    "The attachment id could not be found",
                    GenerateErrorMessage(
                        fieldName,
                        $"the attachment id could not be found from the provided image data",
                        "please ensure that the image data you provide for the image merge field contains the attachment id and is "
                        + "structured as follows: \"filename:mimeType:attachmentId:imageWidth:imageHeight\""),
                    HttpStatusCode.NotFound,
                    data);

            public static Error AttachmentNotFound(string fieldName, JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "merge.field.value.image.file.attachment.not.found",
                    "File attachment could not be found",
                    GenerateErrorMessage(
                        fieldName,
                        $"the file attachment referenced by the provided attachment id could not be found",
                        "please ensure that the attachment id you provide references an existing file"),
                    HttpStatusCode.NotFound,
                    data);

            public static Error InvalidImageAttachment(string fieldName, JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "merge.field.value.image.file.attachment.invalid",
                    "The resolved file attachment is not an image",
                    GenerateErrorMessage(
                        fieldName,
                        $"the file attachment referenced by the provided attachment id is not an image file",
                        "please ensure that the attachment id you provide references a valid image file"),
                    HttpStatusCode.BadRequest,
                    data);

            private static string GenerateErrorMessage(string fieldName, string reason, string resolution)
            {
                return $"When trying to read the source image data for the \"{fieldName}\" image merge field, "
                        + $"the attempt failed because {reason}. To resolve this issue, {resolution}. If you need "
                        + $"further assistance, please contact customer support";
            }
        }

        public static class WordDocumentFieldMerging
        {
            public static Error InvalidFileFormat(
                string fieldName,
                string fileName,
                string supportedExtensions,
                JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "merge.field.value.word.content.file.format.invalid",
                    "The content source file had an unsupported filename extension",
                    $"When trying to read the source file for the \"{fieldName}\" word content merge field, "
                        + $"the attempt failed because the source file (\"{fileName}\") has a filename extension that "
                        + "is not supported. To resolve this issue, please ensure that the attached file has a supported "
                        + $"filename extension ({supportedExtensions}). If you need further assistance please contact "
                        + "technical support.",
                    HttpStatusCode.BadRequest,
                    data);
        }

        public static class HtmlContentFieldMerging
        {
            public static Error InvalidFileFormat(
                string fieldName,
                string fileName,
                JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "merge.field.value.html.content.file.format.invalid",
                    "The content source file had an unsupported filename extension",
                    $"When trying to read the source file for the \"{fieldName}\" HTML content merge field, "
                        + $"the attempt failed because the source file (\"{fileName}\") has a filename extension that "
                        + "is not supported. To resolve this issue, please ensure that the attached HTML file has the correct "
                        + $"filename extension (\"html\"). If you need further assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data);

            public static Error InvalidContent(string fieldName, JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "merge.field.value.html.content.file.content.invalid",
                    "The HTML file content contained invalid HTML",
                    $"When trying to read the HTML file content for the \"{fieldName}\" HTML content merge field, the attempt "
                        + "failed because the file did not contain valid HTML with a supported structure. To resolve this issue, please "
                        + $"ensure that the value provided for  the \"{fieldName}\" HTML content merge field is valid HTML and that "
                        + "it contains at least one block element such as div, p, h1, h2 or similar. If you need further "
                        + "assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data);
        }

        public static class ImageContentFieldMerging
        {
            public static Error InvalidFileFormat(
                string fieldName,
                string fileName,
                JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "merge.field.value.image.content.file.format.invalid",
                    "The content source file had an unsupported filename extension",
                    $"When trying to read the source file for the \"{fieldName}\" image content merge field, the attempt failed "
                        + $"because the source file (\"{fileName}\") has a filename extension that is not supported. To resolve "
                        + "this issue, please ensure that the attached image file has a supported filename extension (\"png\", "
                        + $"\"jpg\", \"jpeg\" or \"gif\"). If you need further assistance please contact customer support.",
                    HttpStatusCode.BadRequest,
                    data);

            public static Error InvalidContent(string fieldName, JObject data) =>
                AutomationError.GenerateErrorWithAdditionalDetailsFromData(
                    "merge.field.value.image.content.invalid",
                    "The file content could not be loaded as image",
                    $"When trying to load the image using the file content for the \"{fieldName}\" image content merge field, "
                        + "the attempt failed because the file content is not a valid image. To resolve this issue, please "
                        + $"ensure that the file provided for the content merge field is a valid image file. If you need further "
                        + "assistance please contact technical support.",
                    HttpStatusCode.BadRequest,
                    data);
        }
    }
}
